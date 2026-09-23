using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using OutlayApp.API.Auth;
using OutlayApp.API.Options.DatabaseOptions;
using OutlayApp.Application.Configuration.Database;
using OutlayApp.Infrastructure.BackgroundJobs;
using OutlayApp.Infrastructure.Database;
using OutlayApp.Infrastructure.KeyVault;
using OutlayApp.Infrastructure.Live;
using OutlayApp.Infrastructure.Monobank;
using OutlayApp.Infrastructure.Processing;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAzureWebAppDiagnostics();
builder.Configuration.AddKeyVault(builder.Environment.IsProduction());

builder.Services
    .AddControllers(o => o.Filters.Add<CardOwnershipFilter>()).Services
    .AddProblemDetails()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(o =>
    {
        o.AddSecurityDefinition("Session", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http, Scheme = "bearer",
            Description = "Session token from POST /api/auth/session",
        });
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Session" } }] = Array.Empty<string>(),
        });
    })
    .AddMonobank(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddBackgroundJobs()
    .AddLiveUpdates()
    .AddMemoryCache();

builder.Services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SessionAuthenticationHandler>(
        SessionAuthenticationHandler.SchemeName, null);
// every endpoint needs a session unless it says [AllowAnonymous]
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(RateLimits.Connect, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origins)
    .AllowAnyMethod()
    .AllowAnyHeader()));

builder.Services.AddOptions<DatabaseOptions>().BindConfiguration(DbConnectionConstants.ConnectionString);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule<ServicesModule>();
        containerBuilder.RegisterModule<MediatorModule>();
        containerBuilder.RegisterModule<ProcessingModule>();
        containerBuilder.RegisterModule<DataAccessModule>();
    });

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
