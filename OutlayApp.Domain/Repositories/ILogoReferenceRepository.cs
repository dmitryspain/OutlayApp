using OutlayApp.Domain.CompanyLogoReferences;

namespace OutlayApp.Domain.Repositories;

public interface ILogoReferenceRepository : IRepository<LogoReference>
{
    Task AddAsync(LogoReference logoReference, CancellationToken cancellationToken = default);
    Task<LogoReference> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<LogoReference> GetByName(string name, CancellationToken cancellationToken = default);
    Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default);
}