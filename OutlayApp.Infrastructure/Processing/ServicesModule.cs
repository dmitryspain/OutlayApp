using Autofac;
using OutlayApp.Infrastructure.Services;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Processing;

public class ServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GoogleImageSearchService>()
            .As<IGoogleImageSearchService>()
            .InstancePerLifetimeScope();
    }
}