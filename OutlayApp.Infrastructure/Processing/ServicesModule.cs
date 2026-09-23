using Autofac;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.LogoReferences;
using OutlayApp.Application.Transactions;
using OutlayApp.Infrastructure.Mcc;
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

        builder.RegisterType<MccDirectory>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<TransactionEnricher>()
            .As<ITransactionEnricher>()
            .InstancePerLifetimeScope();
    }
}
