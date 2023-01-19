using Microsoft.Extensions.DependencyInjection;

namespace OutlayApp.Infrastructure.Mapper;

public static class DependencyInjection
{
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(AssemblyReference.Assembly);
        return services;
    }
}