using Autofac;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Application.Transactions;
using OutlayApp.Infrastructure.Services;

namespace OutlayApp.Infrastructure.Processing;

public class ServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GoogleImageSearchService>()
            .As<ICompanyLogoFinder>()
            .InstancePerLifetimeScope();

        builder.RegisterType<StatementImporter>()
            .AsSelf()
            .InstancePerLifetimeScope();
    }
}