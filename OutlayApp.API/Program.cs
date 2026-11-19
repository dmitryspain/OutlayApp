using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
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

// Behind the Render proxy (and Cloudflare in front of it) the connection comes from a proxy, so the client IP
// for the rate limit and the scheme come from forwarded headers.
var forwardedHeaders = builder.Configuration.GetSection("ForwardedHeaders");
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render does not publish its proxy IPs and the container is reachable only through them, so every hop is trusted.
    o.KnownProxies.Clear();
    o.KnownIPNetworks.Clear();
    // Configurable, so a Render variable can switch the header or the limit without a new build;
    // without the section the defaults stay (X-Forwarded-For, 1).
    if (forwardedHeaders["ForwardedForHeaderName"] is { Length: > 0 } headerName)
        o.ForwardedForHeaderName = headerName;
    o.ForwardLimit = forwardedHeaders.GetValue("ForwardLimit", o.ForwardLimit);
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

// First: HTTPS redirection, CORS and the rate limiter must see the client's scheme and IP.
app.UseForwardedHeaders();
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
// For the host's health check: anonymous and without the database, so it neither needs a session nor wakes Neon.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.Run();
