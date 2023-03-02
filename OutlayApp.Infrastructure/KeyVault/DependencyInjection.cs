using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using OutlayApp.Application.Configuration.BrandFetch;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Infrastructure.BackgroundJobs;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing;
using OutlayApp.Infrastructure.KeyVault;

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