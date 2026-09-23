using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.API.Auth;

/// <summary>
/// Any action argument named cardId / clientCardId must be a card of the signed-in client; otherwise 404
/// (not 403, so card ids cannot be probed).
/// </summary>
public sealed class CardOwnershipFilter : IAsyncActionFilter
{
    private static readonly string[] CardArguments = { "cardId", "clientCardId" };
    private readonly IClientCardsRepository _cards;

    public CardOwnershipFilter(IClientCardsRepository cards)
    {
        _cards = cards;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var name in CardArguments)
        {
            if (!context.ActionArguments.TryGetValue(name, out var value) || value is not Guid cardId)
                continue;
            if (context.HttpContext.User.Identity?.IsAuthenticated != true ||
                !await _cards.BelongsTo(cardId, context.HttpContext.User.ClientId(), context.HttpContext.RequestAborted))
            {
                context.Result = new NotFoundObjectResult(new { code = "ClientCard.NotFound", message = "Картку не знайдено." });
                return;
            }
        }
        await next();
    }
}
