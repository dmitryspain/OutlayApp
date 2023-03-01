using Amazon;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using OutlayApp.API.Options.DatabaseOptions;
using OutlayApp.Application.Configuration.BrandFetch;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Infrastructure.BackgroundJobs;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing;
using OutlayApp.Infrastructure.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHttpClient(MonobankConstants.HttpClient,
        httpClient =>
        {
            var settings = builder.Configuration.GetSection(MonobankConstants.Name)
                .Get<MonobankSettings>();
            httpClient.BaseAddress = new Uri(settings!.BaseUrl);
        }).Services
    .AddInMemoryDbContext()
    .AddDbContext(builder.Configuration)
    .AddBackgroundJobs()
    .AddAutoMapper()
    .AddMemoryCache();


// var env = builder.Environment.EnvironmentName;
// var appName = builder.Environment.ApplicationName;
// var awsSecretsKeyStart = $"{env}_{appName}_";

// builder.Configuration.AddSecretsManager(region: RegionEndpoint.EUWest2,
//     configurator: options =>
//     {
//         options.SecretFilter = entry => entry.Name.StartsWith(awsSecretsKeyStart);
//         options.KeyGenerator = (_, secret) => secret
//             .Replace(awsSecretsKeyStart, string.Empty)
//             .Replace("__", ":");
//     });


var vaultUrl = builder.Configuration["KeyVault:Url"];
var tenantId = builder.Configuration["KeyVault:TenantId"];
var clientId = builder.Configuration["KeyVault:ClientId"];
var secretId = builder.Configuration["KeyVault:ClientSecretId"];

var credential = new ClientSecretCredential(tenantId, clientId, secretId);
var client = new SecretClient(new Uri(vaultUrl!), credential);
builder.Configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());

var secret = client.GetSecret("OutlayAppDbPassword");

builder.Services.AddOptions<DatabaseOptions>().BindConfiguration("Database");
builder.Services.Configure<MonobankSettings>(x => builder.Configuration.GetSection(MonobankConstants.Name).Bind(x));
builder.Services.Configure<BrandFetchSettings>(x =>
    builder.Configuration.GetSection(BrandFetchConstants.Token).Bind(x));
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        var connectionString = builder.Configuration[DbConnectionConstants.ConnectionString]!;
        containerBuilder.RegisterModule<ServicesModule>();
        containerBuilder.RegisterModule<MediatorModule>();
        containerBuilder.RegisterModule<ProcessingModule>();
        containerBuilder.RegisterModule(new DataAccessModule(connectionString));
    });

var app = builder.Build();
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();