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

    public Task AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        return _context.AddAsync(client, cancellationToken).AsTask();
    }

    public void Update(Client client, CancellationToken cancellationToken = default)
    {
        _context.Update(client);
    }

    public Task<Client> GetByPersonalToken(string token, CancellationToken cancellationToken = default)
    {
        return _context.Clients.FirstOrDefaultAsync(x => x.PersonalToken == token, cancellationToken)!;
    }

    public Task<Client> GetById(Guid clientId, CancellationToken cancellationToken = default)
    {
        return _context.Clients.FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken)!;
    }

    public Task<Client> GetByIdWithCards(Guid clientId, CancellationToken cancellationToken = default)
    {
        return _context.Clients.Include(x => x.Cards).FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken)!;
    }
}