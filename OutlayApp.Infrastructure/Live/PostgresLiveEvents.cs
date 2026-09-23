using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OutlayApp.Application.Live;

namespace OutlayApp.Infrastructure.Live;

/// <summary>
/// Live events across instances via Postgres LISTEN/NOTIFY: publishing sends a NOTIFY, and every instance
/// (this one included) hears it and hands it to its own listeners. No extra infrastructure needed.
/// </summary>
public sealed class PostgresLiveEvents : BackgroundService, ILiveEvents
{
    private const string Channel = "outlay_live";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly NpgsqlDataSource _dataSource;
    private readonly LocalLiveHub _hub;
    private readonly ILogger<PostgresLiveEvents> _logger;

    public PostgresLiveEvents(NpgsqlDataSource dataSource, LocalLiveHub hub, ILogger<PostgresLiveEvents> logger)
    {
        _dataSource = dataSource;
        _hub = hub;
        _logger = logger;
    }

    public async Task Publish(Guid cardId, LiveEvent liveEvent)
    {
        var payload = JsonSerializer.Serialize(new Envelope(cardId, liveEvent.Type, JsonSerializer.SerializeToElement(liveEvent.Data, Json)), Json);
        try
        {
            await using var cmd = _dataSource.CreateCommand("SELECT pg_notify(@channel, @payload)");
            cmd.Parameters.AddWithValue("channel", Channel);
            cmd.Parameters.AddWithValue("payload", payload);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // the database is the bus; if it is down, at least this instance's listeners get it
            _logger.LogWarning(ex, "NOTIFY failed, delivering locally only");
            _hub.Deliver(cardId, liveEvent);
        }
    }

    public ILiveSubscription Subscribe(Guid cardId) => _hub.Subscribe(cardId);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync(stoppingToken);
                connection.Notification += (_, e) => OnNotification(e.Payload);
                await using (var listen = new NpgsqlCommand($"LISTEN {Channel}", connection))
                    await listen.ExecuteNonQueryAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                    await connection.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Live events listener lost its connection; reconnecting");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void OnNotification(string payload)
    {
        try
        {
            var e = JsonSerializer.Deserialize<Envelope>(payload, Json);
            if (e is not null)
                _hub.Deliver(e.CardId, new LiveEvent(e.Type, e.Data));
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unreadable live event");
        }
    }

    private sealed record Envelope(Guid CardId, string Type, JsonElement Data);
}
