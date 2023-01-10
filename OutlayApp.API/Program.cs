using MediatR;
using OutlayApp.Application.Configuration.BrandFetch;
using OutlayApp.Application.Configuration.Extensions;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.Repositories;

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
    .AddMediatR(OutlayApp.Application.AssemblyReference.Assembly)
    .AddAutoMapper();

builder.Services.Configure<MonobankSettings>(x => builder.Configuration.GetSection(MonobankConstants.Name).Bind(x));
builder.Services.Configure<BrandFetchSettings>(x => builder.Configuration.GetSection(BrandFetchConstants.Token).Bind(x));
// builder.Services.AddScoped<IBrandFetchService, BrandFetchService>();
// builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IClientTransactionRepository, ClientTransactionRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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