namespace OutlayApp.Infrastructure.Services.Interfaces;

public interface IGoogleImageSearchService
{
    Task<string> GetCompanyLogo(string logoName, int mcc, CancellationToken cancellationToken);
}