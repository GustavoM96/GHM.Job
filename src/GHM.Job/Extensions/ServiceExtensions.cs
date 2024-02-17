using Microsoft.Extensions.DependencyInjection;

namespace GHM.Job.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGhmJob(this IServiceCollection services, Action<GhmJobOptions>? optionsAction = null)
    {
        var options = new GhmJobOptions();
        if (optionsAction is not null)
        {
            optionsAction(options);
        }

        services.AddScoped(sp => options.TimeZoneStrategy);
        services.AddScoped(typeof(IJobErrorHandler<>), options.JobErrorHandler);
        services.AddScoped(typeof(IJobSuccessHandler<>), options.JobSuccessHandler);
        services.AddScoped(typeof(IJobServiceHandler<>), options.JobServiceHandler);
        services.AddScoped(typeof(IJobService<>), typeof(JobService<>));

        return services;
    }
}
