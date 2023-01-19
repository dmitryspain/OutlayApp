using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.Clients.Events;

public sealed record CardsHasBeenAddedEvent(Guid ClientCardId) : DomainEventBase(DateTime.Now);
