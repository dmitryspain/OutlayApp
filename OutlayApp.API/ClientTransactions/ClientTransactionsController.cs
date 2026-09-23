using MediatR;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

namespace OutlayApp.API.ClientTransactions;

[ApiController]
[Route("api/transactions")]
public class ClientTransactionsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IBackfillQueue _backfill;

    public ClientTransactionsController(ISender mediator, IBackfillQueue backfill)
    {
        _mediator = mediator;
        _backfill = backfill;
    }

    /// <summary>Starts loading older history from Monobank in the background (about a minute per month).</summary>
    [HttpPost("backfill")]
    public IActionResult StartBackfill(Guid cardId, int months = 6)
    {
        return Accepted(_backfill.Enqueue(cardId, months));
    }

    [HttpGet("backfill")]
    public IActionResult GetBackfill(Guid cardId)
    {
        return Ok(_backfill.Get(cardId));
    }
    
    [HttpGet("latest")]
    public async Task<IActionResult> FetchLatestTransactions(Guid cardId, CancellationToken cancellationToken)
    {
        var command = new FetchLatestTransactionsCommand(cardId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("by-period")]
    public async Task<IActionResult> GetTransactionsByPeriod(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsQuery(clientCardId, dateFrom, dateTo);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("grouped")]
    public async Task<IActionResult> GetTransactionsGrouped(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsGroupedQuery(clientCardId, dateFrom, dateTo);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("by-description")]
    public async Task<IActionResult> GetTransactionsByDescription(Guid clientCardId, string description, 
        DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsByDescriptionQuery(clientCardId, description, dateFrom, dateTo);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyTransactions(Guid clientCardId, int weeksCount, int skipWeeks, CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsWeeklyQuery(clientCardId, weeksCount, skipWeeks);
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}