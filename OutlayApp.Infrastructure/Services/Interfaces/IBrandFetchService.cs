namespace OutlayApp.Infrastructure.Services.Interfaces;

public interface IBrandFetchService
{
    Task<string> GetCompanyLogo(string companyName, CancellationToken cancellationToken);
}