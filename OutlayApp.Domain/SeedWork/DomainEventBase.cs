namespace OutlayApp.Domain.SeedWork
{
    public record DomainEventBase(DateTime OccurredOn) : IDomainEvent;
}