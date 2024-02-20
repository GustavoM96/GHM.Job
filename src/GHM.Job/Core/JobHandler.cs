namespace GHM.Job;

public class JobHandlerDefault<TRequest> : IJobHandler<TRequest>
{
    public void HandleBeforeExecuter(TRequest request, object? requestId) { }

    public async Task<ExecuterResponse<TResponse>> HandleExecuter<TResponse>(
        Func<TRequest, Task<ExecuterResponse<TResponse>>> requester,
        TRequest request,
        object? requestId
    ) => await requester(request);

    public async Task<RequesterResponse<TRequest>> HandleRequester(Func<Task<RequesterResponse<TRequest>>> requester) =>
        await requester();

    public async Task<UpdaterResponse<TResponse>> HandleUpdater<TResponse>(
        Func<TRequest, Task<UpdaterResponse<TResponse>>> updater,
        TRequest request,
        object? requestId
    ) => await updater(request);
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
