using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.Repositories;

public interface IRepository<T> where T : IAggregateRoot
{
}