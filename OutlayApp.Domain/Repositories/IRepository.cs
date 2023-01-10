using OutlayApp.Domain.SeedWork;

namespace OutlayApp.Domain.Repositories;

public interface IRepository<T> where T : IAggregateRoot
{
}