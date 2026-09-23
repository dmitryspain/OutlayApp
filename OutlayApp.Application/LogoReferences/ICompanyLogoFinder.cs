namespace OutlayApp.Application.LogoReferences;

public interface ICompanyLogoFinder
{
    /// <summary>The logo URL; "" when the search found none; null when it could not search (no key, quota, network).</summary>
    Task<string?> GetCompanyLogo(string logoName, CancellationToken cancellationToken);
}
