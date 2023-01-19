namespace OutlayApp.Domain.Primitives
{
    public record DomainEventBase(DateTime OccurredOn) : IDomainEvent;
}