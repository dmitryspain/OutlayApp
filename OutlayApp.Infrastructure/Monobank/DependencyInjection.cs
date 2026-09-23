using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Application.Monobank;
using Polly;

namespace OutlayApp.Infrastructure.Monobank;

public static class DependencyInjection
{
    public static IServiceCollection AddMonobank(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonobankSettings>(configuration.GetSection(MonobankConstants.Name));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<MonobankRateLimiter>();
        services.AddScoped<IMonobankClient, MonobankClient>();

        services.AddHttpClient(MonobankConstants.HttpClient, (sp, http) =>
            {
                var baseUrl = configuration.GetSection(MonobankConstants.Name)[nameof(MonobankSettings.BaseUrl)];
                http.BaseAddress = new Uri(baseUrl ?? "https://api.monobank.ua");
            })
            .AddStandardResilienceHandler(o =>
            {
                o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
                o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(40);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(40);
                o.Retry.MaxRetryAttempts = 2;
                // a 429 is the per-minute limit: retrying within seconds only extends it
                o.Retry.ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.StatusCode != HttpStatusCode.TooManyRequests &&
                    HttpClientResiliencePredicates.IsTransient(args.Outcome));
            });
        return services;
    }
}
