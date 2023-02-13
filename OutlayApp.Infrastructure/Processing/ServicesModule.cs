using Autofac;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Infrastructure.Services;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Processing;

public class ServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GoogleImageSearchService>()
            .As<ICompanyLogoFinder>()
            .InstancePerLifetimeScope();
    }
}