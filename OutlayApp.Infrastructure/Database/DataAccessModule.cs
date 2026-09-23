using Autofac;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Infrastructure.Database;

public class DataAccessModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(AssemblyReference.Assembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        // OutlayContext itself comes from AddDatabase (one registration, one pooled data source)
    }
}
