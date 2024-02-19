namespace GHM.Job.Extensions;

public class GhmJobOptions
{
    public ITimeZoneStrategy TimeZoneStrategy { get; set; } = new NowTimeZoneStrategy();
    public Type JobHandler { get; set; } = typeof(JobHandlerDefault<>);
    public Type JobServiceHandler { get; set; } = typeof(JobServiceHandlerDefault<>);
}
