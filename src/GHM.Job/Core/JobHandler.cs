namespace GHM.Job;

public class JobHandlerDefault<TRequest> : IJobHandler<TRequest>
{
    public async Task<ExecuterResponse<TRequest, TResponse>> HandleExecuter<TResponse>(
        Func<Task<ExecuterResponse<TRequest, TResponse>>> executer
    ) => await executer();

    public async Task<RequesterResponse<TRequest>> HandleRequester(Func<Task<RequesterResponse<TRequest>>> requester) =>
        await requester();

    public async Task<UpdaterResponse<TRequest>> HandleUpdater(Func<Task<UpdaterResponse<TRequest>>> updater) =>
        await updater();
}

public class JobServiceHandlerDefault<TRequest> : IJobServiceHandler<TRequest>
{
    public async Task<JobServiceResponse<TRequest>> HandleWork(Func<Task<JobServiceResponse<TRequest>>> runWork) =>
        await runWork();
}

public class JobHandler<TRequest>
{
    public JobHandler(IJobServiceHandler<TRequest> service, IJobHandler<TRequest> job)
    {
        Service = service;
        Job = job;
    }

    private JobHandler()
    {
        Service = new JobServiceHandlerDefault<TRequest>();
        Job = new JobHandlerDefault<TRequest>();
    }

    public IJobServiceHandler<TRequest> Service { get; init; }
    public IJobHandler<TRequest> Job { get; init; }

    public static JobHandler<TRequest> Default => new();
}
