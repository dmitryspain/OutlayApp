using MediatR;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Internal.TypeHandlers.FullTextSearchHandlers;
using OutlayApp.Application.ChooseClientCards.Commands;
using OutlayApp.Application.ClientCards.Command;
using OutlayApp.Application.Clients.Commands;
using OutlayApp.Application.Clients.Queries.GetClientInfo;

namespace OutlayApp.API.Clients;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterClient(string clientToken, CancellationToken cancellationToken)
    {
        var command = new RegisterClientCommand(clientToken);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpPost("cards")]
    public async Task<IActionResult> GetCards(string clientToken,CancellationToken cancellationToken)
    {
        var command = new ChooseClientCardsCommand(clientToken);
        var result = await _sender.Send(command,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
     
    [HttpGet("personal-info")]
    public async Task<IActionResult> GetClientInfo(Guid clientId, CancellationToken cancellationToken)
    {
        var command = new GetClientQuery(clientId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }
    [HttpGet("update-balance")]
    public async Task<IActionResult> UpdateBalance(string clientToken, CancellationToken cancellationToken)
    {
        var command = new UpdateBalanceCommand(clientToken);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }
}