using Autofac;
using Microsoft.EntityFrameworkCore;
using OutlayApp.Domain.Repositories;

namespace OutlayApp.Infrastructure.Database;

public class DataAccessModule : Module
{
    private readonly string _connectionString;

    public DataAccessModule(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(AssemblyReference.Assembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces();
        
        builder.Register(c =>
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<OutlayContext>();
                dbContextOptionsBuilder.UseNpgsql(_connectionString);
                return new OutlayContext(dbContextOptionsBuilder.Options);
            })
            .AsSelf()
            .As<DbContext>()
            .InstancePerLifetimeScope();
    }
}