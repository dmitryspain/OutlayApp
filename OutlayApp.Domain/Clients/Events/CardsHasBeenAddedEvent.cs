using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.Clients.Events;

public sealed record CardsHasBeenAddedEvent(Guid ClientId) : DomainEventBase(DateTime.Now);
