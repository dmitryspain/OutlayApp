using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace OutlayApp.Infrastructure.KeyVault;

public static class DependencyInjection
{
    public static ConfigurationManager AddKeyVault(this ConfigurationManager configuration, bool isProduction)
    {
        if (!isProduction)
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