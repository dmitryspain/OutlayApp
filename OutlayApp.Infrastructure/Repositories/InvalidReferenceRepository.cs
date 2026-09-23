using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.CompanyLogoReferences;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;

namespace OutlayApp.Infrastructure.Repositories;

public class InvalidReferenceRepository : IInvalidReferenceRepository
{
    private readonly OutlayContext _context;

    public InvalidReferenceRepository(OutlayContext context)
    {
        _context = context;
    }

    public Task AddAsync(InvalidReference reference, CancellationToken cancellationToken = default) =>
        _context.AddAsync(reference, cancellationToken).AsTask();

    public Task<InvalidReference?> GetById(Guid id, CancellationToken cancellationToken = default) =>
        _context.InvalidReferences.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default) =>
        _context.InvalidReferences.AnyAsync(x => x.Name == name, cancellationToken);
}
