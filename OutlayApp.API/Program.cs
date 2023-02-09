using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using OutlayApp.Application.Configuration.BrandFetch;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Application.Configuration.Extensions;
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
            httpClient.DefaultRequestHeaders.Add(MonobankConstants.TokenHeader, settings.PersonalToken);
        }).Services
    .AddInMemoryDbContext()
    .AddDbContext(builder.Configuration)
    .AddRedis(builder.Configuration)
    .AddBackgroundJobs()
    .AddAutoMapper();


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

// var region = "us-east-1";
//
// var secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
// var request = new GetSecretValueRequest()
// {
//     SecretId = "ApiKey",
//     VersionStage = "AWSCURRENT"
// };
// var response = await secretsManagerClient.GetSecretValueAsync(request);
// var res = response.SecretString;


// var awsOptions = builder.Configuration.GetAWSOptions();
// builder.Services.AddDefaultAWSOptions(awsOptions);
// builder.Services.AddAWSService<IAmazonDynamoDB>();
// builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

builder.Services.Configure<MonobankSettings>(x => builder.Configuration.GetSection(MonobankConstants.Name).Bind(x));
builder.Services.Configure<BrandFetchSettings>(x =>
    builder.Configuration.GetSection(BrandFetchConstants.Token).Bind(x));
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule<ServicesModule>();
        containerBuilder.RegisterModule<MediatorModule>();
        containerBuilder.RegisterModule<ProcessingModule>();
        containerBuilder.RegisterModule(
            new DataAccessModule(builder.Configuration[DbConnectionConstants.ConnectionString]!));
    });

var app = builder.Build();
if (app.Environment.IsDevelopment())
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