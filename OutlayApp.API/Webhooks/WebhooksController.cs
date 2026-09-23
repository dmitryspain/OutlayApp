using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Webhooks;

namespace OutlayApp.API.Webhooks;

/// <summary>Monobank's push endpoint. The secret path segment keeps anyone else from posting fake transactions.</summary>
[ApiController]
[Route(WebhookUrl.Route)]
public class WebhooksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly MonobankSettings _settings;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(ISender sender, IOptions<MonobankSettings> settings, ILogger<WebhooksController> logger)
    {
        _sender = sender;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>Monobank checks the URL with a GET before accepting it.</summary>
    [HttpGet("{secret}")]
    public IActionResult Verify(string secret) => IsOurs(secret) ? Ok() : NotFound();

    [HttpPost("{secret}")]
    public async Task<IActionResult> Receive(string secret, [FromBody] MonobankWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        if (!IsOurs(secret))
            return NotFound();
        if (payload.Type != MonobankWebhookPayload.StatementItemType || payload.Data?.StatementItem is null)
            return Ok();

        // a non-200 makes Monobank retry (and after repeated failures, drop the webhook), so only real errors fail
        var result = await _sender.Send(
            new ProcessStatementItemCommand(payload.Data.Account, payload.Data.StatementItem), cancellationToken);
        if (result.IsFailure)
            _logger.LogWarning("Webhook item not processed: {Error}", result.Error.Message);
        return Ok();
    }

    private bool IsOurs(string secret) =>
        !string.IsNullOrEmpty(_settings.WebhookSecret) &&
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(_settings.WebhookSecret));
}
