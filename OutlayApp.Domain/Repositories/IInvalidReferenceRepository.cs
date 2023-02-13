using OutlayApp.Domain.CompanyLogoReferences;

namespace OutlayApp.Domain.Repositories;

public interface IInvalidReferenceRepository : IRepository<LogoReference>
{
    Task AddAsync(InvalidReference reference, CancellationToken cancellationToken = default);
    Task<InvalidReference> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default);
}