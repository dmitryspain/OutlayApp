using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("personal-info")]
    public async Task<IActionResult> GetClientInfo(Guid clientId, CancellationToken cancellationToken)
    {
        var command = new GetClientQuery(clientId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }
}