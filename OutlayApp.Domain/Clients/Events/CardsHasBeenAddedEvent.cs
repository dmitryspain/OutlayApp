using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.Clients.Events;

public sealed class CardsHasBeenAddedEvent : DomainEventBase
{
    public Guid ClientId { get; }

    public CardsHasBeenAddedEvent(Guid clientId)
    {
        ClientId = clientId;
    }
}