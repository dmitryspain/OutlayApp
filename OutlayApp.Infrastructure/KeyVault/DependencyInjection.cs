using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace OutlayApp.Infrastructure.KeyVault;

public static class DependencyInjection
{
    /// <summary>
    /// Adds Azure Key Vault in Production, but only when its client secret is set: hosts without Azure (Render)
    /// keep their secrets in environment variables, and an empty secret would fail the start.
    /// </summary>
    public static ConfigurationManager AddKeyVault(this ConfigurationManager configuration, bool isProduction)
    {
        if (!isProduction || string.IsNullOrWhiteSpace(configuration[KeyVaultConstants.ClientSecretId]))
            return configuration;

        var vaultUrl = configuration[KeyVaultConstants.Url];
        var tenantId = configuration[KeyVaultConstants.TenantId];
        var clientId = configuration[KeyVaultConstants.ClientId];
        var secretId = configuration[KeyVaultConstants.ClientSecretId];

        var credential = new ClientSecretCredential(tenantId, clientId, secretId);
        var client = new SecretClient(new Uri(vaultUrl!), credential);
        configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());

        return configuration;
    }
}