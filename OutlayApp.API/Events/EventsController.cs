using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Live;

namespace OutlayApp.API.Events;

/// <summary>
/// Server-sent events for one of the client's cards: new transactions (from the webhook) and history progress.
/// EventSource cannot send headers, so the session token comes as ?access_token=.
/// </summary>
[ApiController]
[Authorize]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private static readonly TimeSpan Heartbeat = TimeSpan.FromSeconds(25);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly ILiveEvents _liveEvents;

    public EventsController(ILiveEvents liveEvents)
    {
        _liveEvents = liveEvents;
    }

    [HttpGet]
    public async Task Stream(Guid cardId, CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        using var subscription = _liveEvents.Subscribe(cardId);
        await Write(": connected\n\n", cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            using var wait = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            wait.CancelAfter(Heartbeat);
            try
            {
                if (!await subscription.Reader.WaitToReadAsync(wait.Token))
                    return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // idle: a comment line keeps proxies from closing the connection
                await Write(": ping\n\n", cancellationToken);
                continue;
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (subscription.Reader.TryRead(out var e))
                await Write($"event: {e.Type}\ndata: {JsonSerializer.Serialize(e.Data, Json)}\n\n", cancellationToken);
        }
    }

    private async Task Write(string text, CancellationToken cancellationToken)
    {
        await Response.WriteAsync(text, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
