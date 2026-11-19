using Microsoft.Extensions.Configuration;
using OutlayApp.Infrastructure.KeyVault;

namespace OutlayApp.Tests;

/// <summary>Production on a host without Azure (Render): secrets come from environment variables, not Key Vault.</summary>
public class KeyVaultTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Production_without_a_client_secret_adds_no_key_vault(string? secret)
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [KeyVaultConstants.Url] = "https://outlayapp.vault.azure.net/",
            [KeyVaultConstants.TenantId] = "058f9faa-280d-40bd-a9e0-6278f5505fe2",
            [KeyVaultConstants.ClientId] = "1ddd61e3-76ca-4740-bc16-b2f8e69f62af",
            [KeyVaultConstants.ClientSecretId] = secret,
        });
        IConfigurationBuilder builder = configuration;
        var sources = builder.Sources.Count;

        configuration.AddKeyVault(isProduction: true);

        Assert.Equal(sources, builder.Sources.Count);
    }
}
