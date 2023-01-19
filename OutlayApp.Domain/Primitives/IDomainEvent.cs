using MediatR;

namespace OutlayApp.Domain.Primitives
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}