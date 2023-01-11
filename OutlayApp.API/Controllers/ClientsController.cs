using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlayApp.Application.Clients.Commands;
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
    public async Task<IActionResult> FetchLatestTransactions(Guid clientId, Guid clientCardId, CancellationToken cancellationToken)
    {
        //todo find user
        var command = new FetchLatestTransactionsCommand(clientId, clientCardId);
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

    
    // public OutlayController(ICardService cardService, IDistributedCache cache)
    // {
    //     _cardService = cardService;
    //     _cache = cache;
    // }

    // [HttpGet("client-info")]
    // public async Task<ActionResult<ClientInfo>> GetClientInfo(CancellationToken cancellationToken)
    // {
    //     var clientInfo = await _cardService.GetClientInfo(cancellationToken);
    //     clientInfo.CardInfos = clientInfo.CardInfos.Where(x => x.Balance > 0); 
    //     return Ok(clientInfo);
    // }
    
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