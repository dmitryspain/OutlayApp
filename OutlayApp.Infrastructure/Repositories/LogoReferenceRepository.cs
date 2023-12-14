using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.CompanyLogoReferences;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class LogoReferenceRepository : ILogoReferenceRepository
{
    private readonly OutlayContext _context;

    public LogoReferenceRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task AddAsync(LogoReference logoReference, CancellationToken cancellationToken = default)
    {
        return _context.AddAsync(logoReference, cancellationToken).AsTask();
    }

    public Task<LogoReference> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.LogoReferences.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)!;
    }

    public Task<LogoReference> GetByName(string name, CancellationToken cancellationToken = default)
    {
        return _context.LogoReferences.FirstOrDefaultAsync(x => x.Name == name, cancellationToken)!;
    }

    public Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _context.LogoReferences.Select(x => x.Name).ContainsAsync(name, cancellationToken);
    }
}