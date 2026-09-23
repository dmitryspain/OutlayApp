using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.API.Auth;
using OutlayApp.Application.ClientCards.Command;
using OutlayApp.Application.Clients.Queries.GetClientCards;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Application.Webhooks;

namespace OutlayApp.API.Clients;

/// <summary>The signed-in client: profile, cards, balances, webhook. Nothing here takes the Monobank token.</summary>
[ApiController]
[Authorize]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetClientQuery(User.ClientId()), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("cards")]
    public async Task<IActionResult> GetCards(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetClientCardsQuery(User.ClientId()), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Re-reads balances (and any new account) from the bank.</summary>
    [HttpPost("balance/refresh")]
    public async Task<IActionResult> RefreshBalance(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateBalanceCommand(User.ClientId()), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>Asks Monobank to push this client's transactions to us.</summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> RegisterWebhook(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterWebhookCommand(User.ClientId()), cancellationToken);
        return result.IsSuccess ? Ok(new { url = result.Value }) : BadRequest(result.Error);
    }

    [HttpGet("webhook")]
    public async Task<IActionResult> GetWebhookStatus(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWebhookStatusQuery(User.ClientId()), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
