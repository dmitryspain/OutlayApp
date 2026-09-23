using System.Reflection;
using Autofac;
using MediatR;
using MediatR.NotificationPublishers;
using Module = Autofac.Module;

namespace OutlayApp.Infrastructure.Processing;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // MediatR 12 resolves handlers through IServiceProvider (Autofac supplies it); notifications run one by one
        builder.RegisterType<ForeachAwaitPublisher>().As<INotificationPublisher>().SingleInstance();
        builder.RegisterType<Mediator>()
            .As<IMediator>().As<ISender>().As<IPublisher>()
            .UsingConstructor(typeof(IServiceProvider), typeof(INotificationPublisher))
            .InstancePerLifetimeScope();

        var mediatrOpenTypes = new[]
        {
            typeof(IRequestHandler<,>),
            typeof(INotificationHandler<>)
        };

        foreach (var mediatrOpenType in mediatrOpenTypes)
        {
            builder
                .RegisterAssemblyTypes(Application.AssemblyReference.Assembly, AssemblyReference.Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .FindConstructorsWith(new AllConstructorFinder())
                .AsImplementedInterfaces();
        }
    }
}
