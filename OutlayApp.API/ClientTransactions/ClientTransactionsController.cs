using MediatR;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;

namespace OutlayApp.API.ClientTransactions;

[ApiController]
[Route("api/transactions")]
public class ClientTransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientTransactionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> FetchLatestTransactions(string externalCardId, CancellationToken cancellationToken)
    {
        var command = new FetchLatestTransactionsCommand(externalCardId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
    

    [HttpGet("by-period")]
    public async Task<IActionResult> GetTransactionsByPeriod(Guid clientCardId, long? dateFrom, long? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsQuery(clientCardId, dateFrom, dateTo);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    
    [HttpGet("grouped")]
    public async Task<IActionResult> GetTransactionsGrouped(Guid clientCardId, long? dateFrom, long? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsGroupedQuery(clientCardId, dateFrom, dateTo);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    
    [HttpGet("by-description")]
    public async Task<IActionResult> GetTransactionsByDescription(Guid clientCardId, string description,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsByDescriptionQuery(clientCardId, description);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}