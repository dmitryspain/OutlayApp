using OutlayApp.Domain.Clients;

namespace OutlayApp.Domain.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    /// <summary>With cards.</summary>
    Task<Client?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
    Task<Client?> GetById(Guid clientId, CancellationToken cancellationToken = default);
    Task<Client?> GetByIdWithCards(Guid clientId, CancellationToken cancellationToken = default);
}
