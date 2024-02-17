namespace GHM.Job.Extensions;

public class GhmJobOptions
{
    public ITimeZoneStrategy TimeZoneStrategy { get; set; } = new NowTimeZoneStrategy();
    public Type JobErrorHandler { get; set; } = typeof(JobErrorHandlerDefault<>);
    public Type JobSuccessHandler { get; set; } = typeof(JobSuccessHandlerDefault<>);
    public Type JobServiceHandler { get; set; } = typeof(JobServiceHandlerDefault<>);
}
