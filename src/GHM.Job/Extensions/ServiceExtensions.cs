using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GHM.Job.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGhmJob(this IServiceCollection services)
    {
        services.AddScoped<ITimeZoneStrategy, NowTimeZoneStrategy>();
        services.AddScoped(typeof(JobHandler<>));
        services.AddScoped(typeof(IJobErrorHandler<>), typeof(JobErrorHandlerDefault<>));
        services.AddScoped(typeof(IJobSuccessHandler<>), typeof(JobSuccessHandlerDefault<>));
        services.AddScoped(typeof(IJobServiceHandler<>), typeof(JobServiceHandlerDefault<>));
        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }

    public static IServiceCollection AddGhmJob(this IServiceCollection services, ITimeZoneStrategy timeZoneStrategy)
    {
        services.AddScoped(sp => timeZoneStrategy);
        services.AddScoped(typeof(JobHandler<>));
        services.AddScoped(typeof(IJobErrorHandler<>), typeof(JobErrorHandlerDefault<>));
        services.AddScoped(typeof(IJobSuccessHandler<>), typeof(JobSuccessHandlerDefault<>));
        services.AddScoped(typeof(IJobServiceHandler<>), typeof(JobServiceHandlerDefault<>));
        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }

    public static IServiceCollection AddGhmErrorHandler(this IServiceCollection services, Type handlerType)
    {
        var serviceHandlerType = typeof(IJobErrorHandler<>);
        return services.ReplaceService(serviceHandlerType, handlerType, ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddGhmSuccessHandler(this IServiceCollection services, Type handlerType)
    {
        var serviceHandlerType = typeof(IJobSuccessHandler<>);
        return services.ReplaceService(serviceHandlerType, handlerType, ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddGhmServiceHandler(this IServiceCollection services, Type handlerType)
    {
        var serviceHandlerType = typeof(IJobServiceHandler<>);
        return services.ReplaceService(serviceHandlerType, handlerType, ServiceLifetime.Scoped);
    }

    private static IServiceCollection ReplaceService(
        this IServiceCollection services,
        Type serviceType,
        Type implemantationType,
        ServiceLifetime lifetime
    )
    {
        if (!implemantationType.GetInterfaces().Contains(serviceType))
        {
            throw new ArgumentException($"the type {serviceType.Name} does not contain the interface {serviceType.Name}");
        }

        var descriptor = new ServiceDescriptor(serviceType, implemantationType, lifetime);
        return services.Replace(descriptor);
    }
}
