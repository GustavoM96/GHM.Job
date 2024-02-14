using Microsoft.Extensions.DependencyInjection;

namespace GHM.Job.Extensions;

public static class ServiceExtentions
{
    public static IServiceCollection AddGhmJob(this IServiceCollection services)
    {
        services.AddScoped<ITimeZoneStrategy, NowTimeZoneStrategy>();
        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }

    public static IServiceCollection AddGhmJob(this IServiceCollection services, ITimeZoneStrategy timeZoneStrategy)
    {
        services.AddScoped(sp => timeZoneStrategy);
        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }
}
