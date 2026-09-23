using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly OutlayContext _context;

    public ClientRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task AddAsync(Client client, CancellationToken cancellationToken = default) =>
        _context.AddAsync(client, cancellationToken).AsTask();

    public Task<Client?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.Clients.Include(x => x.Cards).FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public Task<Client?> GetById(Guid clientId, CancellationToken cancellationToken = default) =>
        _context.Clients.FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken);

    public Task<Client?> GetByIdWithCards(Guid clientId, CancellationToken cancellationToken = default) =>
        _context.Clients.Include(x => x.Cards).FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken);
}
