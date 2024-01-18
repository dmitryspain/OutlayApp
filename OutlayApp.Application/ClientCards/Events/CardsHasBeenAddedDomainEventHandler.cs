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
        var clientCard = await _clientCardsRepository.GetById(notification.ClientCardId, cancellationToken);
        if (clientCard == null || clientCard.Balance == 0)
            return; 
        
        await _sender.Send(new FetchLatestTransactionsCommand(clientCard.ExternalCardId),
            cancellationToken);
    }
}