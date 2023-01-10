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

    public Task<Client?> GetByPersonalToken(string token, CancellationToken cancellationToken = default)
    {
        return _context.Clients.FirstOrDefaultAsync(x => x.PersonalToken == token, cancellationToken);
    }
}