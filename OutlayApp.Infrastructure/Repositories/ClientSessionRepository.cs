using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Sessions;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class ClientSessionRepository : IClientSessionRepository
{
    private readonly OutlayContext _context;

    public ClientSessionRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task AddAsync(ClientSession session, CancellationToken cancellationToken = default) =>
        _context.ClientSessions.AddAsync(session, cancellationToken).AsTask();

    public Task<ClientSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.ClientSessions.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public void Remove(ClientSession session) => _context.ClientSessions.Remove(session);
}
