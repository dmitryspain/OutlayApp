using System.Data;

namespace OutlayApp.Domain.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    IDbTransaction BeginTransaction();
}