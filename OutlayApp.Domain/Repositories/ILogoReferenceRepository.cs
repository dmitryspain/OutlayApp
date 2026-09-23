using OutlayApp.Domain.CompanyLogoReferences;

namespace OutlayApp.Domain.Repositories;

public interface ILogoReferenceRepository : IRepository<LogoReference>
{
    Task AddAsync(LogoReference logoReference, CancellationToken cancellationToken = default);
    Task<LogoReference?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<LogoReference?> GetByName(string name, CancellationToken cancellationToken = default);
    /// <summary>Logo URL per name, for the names that have one — one query for a whole list.</summary>
    Task<Dictionary<string, string>> GetUrlsByNames(IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);
    Task<bool> ContainsAsync(string name, CancellationToken cancellationToken = default);
}
