using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class ClientCardsRepository : IClientCardsRepository
{
    private readonly OutlayContext _context;

    public ClientCardsRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task AddAsync(ClientCard card, CancellationToken cancellationToken = default) =>
        _context.ClientCards.AddAsync(card, cancellationToken).AsTask();

    public Task<List<ClientCard>> GetAll(Guid clientId, CancellationToken cancellationToken = default) =>
        _context.ClientCards.Where(x => x.ClientId == clientId).OrderByDescending(x => x.Balance)
            .ToListAsync(cancellationToken);

    public Task<ClientCard?> GetByExternalId(string externalId, CancellationToken cancellationToken = default) =>
        _context.ClientCards.FirstOrDefaultAsync(x => x.ExternalCardId == externalId, cancellationToken);

    public Task<ClientCard?> GetById(Guid clientCardId, CancellationToken cancellationToken = default) =>
        _context.ClientCards.FirstOrDefaultAsync(x => x.Id == clientCardId, cancellationToken);

    public Task<bool> BelongsTo(Guid clientCardId, Guid clientId, CancellationToken cancellationToken = default) =>
        _context.ClientCards.AnyAsync(x => x.Id == clientCardId && x.ClientId == clientId, cancellationToken);
}
