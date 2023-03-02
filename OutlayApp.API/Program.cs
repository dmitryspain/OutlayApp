using Autofac;
using Autofac.Extensions.DependencyInjection;
using OutlayApp.API.Options.DatabaseOptions;
using OutlayApp.Application.Configuration.BrandFetch;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Infrastructure.BackgroundJobs;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Processing;
using OutlayApp.Infrastructure.KeyVault;

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

builder.Configuration.AddKeyVault(builder.Environment.IsProduction());
builder.Services.AddOptions<DatabaseOptions>().BindConfiguration(DbConnectionConstants.ConnectionString);
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