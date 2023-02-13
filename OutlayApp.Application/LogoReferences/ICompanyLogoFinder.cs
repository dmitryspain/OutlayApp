namespace OutlayApp.Application.LogoReferences;

public interface ICompanyLogoFinder
{
    Task<string> GetCompanyLogo(string logoName, CancellationToken cancellationToken);
}