using Microsoft.Extensions.DependencyInjection;

namespace GHM.Job.Extensions;

public static class ServiceExtentions
{
    public static IServiceCollection AddGhmJob(this IServiceCollection services)
    {
        services.AddScoped<ITimeZoneStrategy, NowTimeZoneStrategy>();
        services.AddScoped(sp => JobHandler.Default);

        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }

    public static IServiceCollection AddGhmJob(
        this IServiceCollection services,
        ITimeZoneStrategy timeZoneStrategy,
        JobHandler jobHandler
    )
    {
        services.AddScoped(sp => timeZoneStrategy);
        services.AddScoped(sp => jobHandler);

        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));
        return services;
    }
}
