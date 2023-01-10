using OutlayApp.Domain.Clients;

namespace OutlayApp.Domain.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<Client?> GetByPersonalToken(string token, CancellationToken cancellationToken = default);
}