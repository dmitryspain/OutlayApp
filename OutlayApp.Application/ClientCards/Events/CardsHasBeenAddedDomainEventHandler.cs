using MediatR;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Domain.Clients.Events;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Application.ClientCards.Events;

internal sealed class CardsHasBeenAddedDomainEventHandler : INotificationHandler<CardsHasBeenAddedEvent>
{
    private readonly ISender _sender;
    private readonly IClientCardsRepository _clientCardsRepository;

    public CardsHasBeenAddedDomainEventHandler(ISender sender, IClientCardsRepository clientCardsRepository)
    {
        _sender = sender;
        _clientCardsRepository = clientCardsRepository;
    }

    public async Task Handle(CardsHasBeenAddedEvent notification, CancellationToken cancellationToken)
    {
        var cards = await _clientCardsRepository.GetAll(notification.ClientId, cancellationToken);
        foreach (var card in cards)
            await _sender.Send(new FetchLatestTransactionsCommand(notification.ClientId, card.Id), cancellationToken);
    }
}