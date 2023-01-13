using MediatR;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Clients.Commands;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Application.Clients.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Commands;

namespace OutlayApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ISender _sender;

    public ClientsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("latest-transactions")]
    public async Task<IActionResult> FetchLatestTransactions(Guid clientId, string externalCardId, CancellationToken cancellationToken)
    {
        var command = new FetchLatestTransactionsCommand(clientId, externalCardId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("register-client")]
    public async Task<IActionResult> RegisterClient(string clientToken, CancellationToken cancellationToken)
    {
        var command = new RegisterClientCommand(clientToken);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("transactions-by-period")]
    public async Task<IActionResult> TransactionsByPeriod(Guid clientCardId, long? dateFrom, long? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsQuery(clientCardId, dateFrom, dateTo);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    [HttpGet("client-info")]
    public async Task<IActionResult> GetClientInfo(Guid clientId, CancellationToken cancellationToken)
    {
        var command = new GetClientQuery(clientId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    //
    // [HttpGet("card-history")]
    // public async Task<ActionResult<IEnumerable<Transaction>>> GetCardHistory(string cardId,
    //     CancellationToken cancellationToken)
    // {
    //     var cardHistories = await _cardService.GetCardHistory(cardId, cancellationToken);
    //     return Ok(cardHistories);
    // }
    //
    // [HttpGet("card-history-grouped")]
    // public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetCardHistoryGrouped(
    //     [FromQuery] CardHistoryRequest cardRequest,
    //     CancellationToken cancellationToken)
    // {
    //     const string cacheKey = "card-history";
    //     var cached = await _cache.GetRecordAsync<List<TransactionResponse>>(cacheKey);
    //     if (cached is not null)
    //         return Ok(cached);
    //
    //     var transactions = await _cardService.GetCardHistoryGrouped(cardRequest, cancellationToken);
    //     await _cache.SetRecordAsync(cacheKey, absoluteExpireTime: TimeSpan.FromHours(1), data: transactions);
    //     return Ok(transactions);
    // }
    //
    // [HttpGet("card-history-by-description")]
    // public async Task<ActionResult<IEnumerable<TransactionByDescriptionResponse>>> GetCardHistoryByDescription(
    //     [FromQuery] CardHistoryByDescriptionRequest request,
    //     CancellationToken cancellationToken)
    // {
    //     var transactions = await _cardService.GetCardHistoryByDescription(request, cancellationToken);
    //     return Ok(transactions);
    // }
}