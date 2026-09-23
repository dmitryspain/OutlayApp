using MediatR;
using OutlayApp.Application.Abstractions;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.Monobank;
using OutlayApp.Domain.Clients.Events;

namespace OutlayApp.Application.ClientCards.Events;

/// <summary>A new card: load its last month.</summary>
internal sealed class CardsHasBeenAddedDomainEventHandler : INotificationHandler<CardsHasBeenAddedEvent>
{
    private readonly ISender _sender;

    public CardsHasBeenAddedDomainEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(CardsHasBeenAddedEvent notification, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FetchLatestTransactionsCommand(notification.ClientCardId), cancellationToken);
        // several new cards share one token's rate limit: try again on the next outbox run
        if (result.IsFailure && result.Error.Code == MonobankErrors.RateLimited)
            throw new RetryLaterException(result.Error.Message);
    }
}
