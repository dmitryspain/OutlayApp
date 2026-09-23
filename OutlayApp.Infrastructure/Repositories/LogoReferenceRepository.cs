using Microsoft.EntityFrameworkCore;
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

    public Task AddAsync(LogoReference logoReference, CancellationToken cancellationToken = default) =>
        _context.AddAsync(logoReference, cancellationToken).AsTask();

    public Task<LogoReference?> GetById(Guid id, CancellationToken cancellationToken = default) =>
        _context.LogoReferences.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<LogoReference?> GetByName(string name, CancellationToken cancellationToken = default) =>
        _context.LogoReferences.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task<Dictionary<string, string>> GetUrlsByNames(IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        if (names.Count == 0)
            return new Dictionary<string, string>();
        var rows = await _context.LogoReferences.AsNoTracking()
            .Where(x => names.Contains(x.Name))
            .Select(x => new { x.Name, x.Url })
            .ToListAsync(cancellationToken);
        return rows.GroupBy(x => x.Name).ToDictionary(g => g.Key, g => g.First().Url);
    }

    public Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default) =>
        _context.LogoReferences.AnyAsync(x => x.Name == name, cancellationToken);
}
