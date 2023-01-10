using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutlayApp.Application.Configuration.Redis;

namespace OutlayApp.Application.Configuration.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration.GetSection(RedisConstants.Connection).Value;
            option.InstanceName = "master";
        });

        return services;
    }
}