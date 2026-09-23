using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Backfill;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;

namespace OutlayApp.API.ClientTransactions;

/// <summary>Transactions of one of the client's cards (the card is checked by CardOwnershipFilter).</summary>
[ApiController]
[Authorize]
[Route("api/transactions")]
public class ClientTransactionsController : ControllerBase
{
    private readonly ISender _mediator;

    public ClientTransactionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Pulls the newest statement from the bank.</summary>
    [HttpPost("latest")]
    public async Task<IActionResult> FetchLatestTransactions(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FetchLatestTransactionsCommand(cardId), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>Dates with an offset (ISO 8601) are exact; bare dates mean Kyiv time. Times in the reply are UTC.</summary>
    [HttpGet("by-period")]
    public async Task<IActionResult> GetTransactionsByPeriod(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientTransactionsQuery(clientCardId, dateFrom, dateTo), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("grouped")]
    public async Task<IActionResult> GetTransactionsGrouped(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientTransactionsGroupedQuery(clientCardId, dateFrom, dateTo), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("by-description")]
    public async Task<IActionResult> GetTransactionsByDescription(Guid clientCardId, string description,
        DateTime? dateFrom, DateTime? dateTo, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClientTransactionsByDescriptionQuery(clientCardId, description, dateFrom, dateTo), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyTransactions(Guid clientCardId, int weeksCount, int skipWeeks,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientTransactionsWeeklyQuery(clientCardId, weeksCount, skipWeeks), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Starts loading older history in the background (about a minute per month).</summary>
    [HttpPost("backfill")]
    public async Task<IActionResult> StartBackfill(Guid cardId, int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new StartBackfillCommand(cardId, months), cancellationToken);
        return result.IsSuccess ? Accepted(result.Value) : BadRequest(result.Error);
    }

    /// <summary>Re-reads the last days from the bank: fills gaps, drops duplicates.</summary>
    [HttpPost("resync")]
    public async Task<IActionResult> StartResync(Guid cardId, int days = 31, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new StartResyncCommand(cardId, days), cancellationToken);
        return result.IsSuccess ? Accepted(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("backfill")]
    public async Task<IActionResult> GetBackfill(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBackfillStatusQuery(cardId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
