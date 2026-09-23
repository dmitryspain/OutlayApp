using OutlayApp.Domain.Sessions;

namespace OutlayApp.Domain.Repositories;

public interface IClientSessionRepository : IRepository<ClientSession>
{
    Task AddAsync(ClientSession session, CancellationToken cancellationToken = default);
    Task<ClientSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
    void Remove(ClientSession session);
}
