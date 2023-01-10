using Microsoft.Extensions.DependencyInjection;
using OutlayApp.Application.Mapper;

namespace OutlayApp.Application.Configuration.Extensions;

public static class AutoMapperExtensions
{
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TransactionProfile).Assembly);
        return services;
    }
}