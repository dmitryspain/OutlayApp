using MediatR;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Clients.Commands;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.API.Clients;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IGoogleImageSearchService _googleImageSearchService;

    public ClientsController(ISender sender, IGoogleImageSearchService googleImageSearchService)
    {
        _sender = sender;
        _googleImageSearchService = googleImageSearchService;
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
    
    // [HttpGet("test")]
    // public async Task<IActionResult> Test(CancellationToken cancellationToken)
    // {
    //     var query = "uklon";
    //     var res = await _googleImageSearchService.GetCompanyLogo(query, cancellationToken);
    //     return Ok(res);
    // }
}