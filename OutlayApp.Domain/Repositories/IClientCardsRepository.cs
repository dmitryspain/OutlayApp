using OutlayApp.Domain.ClientCards;

namespace OutlayApp.Domain.Repositories;

public interface IClientCardsRepository : IRepository<ClientCard>
{
    Task AddAsync(ClientCard card, CancellationToken cancellationToken = default);
    Task<List<ClientCard>> GetAll(Guid clientId, CancellationToken cancellationToken = default);
}