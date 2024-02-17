namespace GHM.Job;

public class JobErrorHandlerDefault<TRequest> : IJobErrorHandler<TRequest>
{
    public void OnRequesterError(Exception ex) { }

    public void OnExecuterError(Exception ex, TRequest request, object? requestId) { }

    public void OnUpdaterError(Exception ex, TRequest request, object? requestId) { }
}

public class JobSuccessHandlerDefault<TRequest> : IJobSuccessHandler<TRequest>
{
    public void AfterRequester(TRequest request, object? requestId) { }

    public void AfterExecuter(TRequest request, object? requestId) { }

    public void AfterUpdater(TRequest request, object? requestId) { }
}

public class JobServiceHandlerDefault<TRequest> : IJobServiceHandler<TRequest>
{
    public async Task<JobServiceResponse<TRequest>> HandleWork(Func<Task<JobServiceResponse<TRequest>>> runWork) =>
        await runWork();
}

public class JobHandler<TRequest> : IJobHandler<TRequest>
{
    public JobHandler(
        IJobServiceHandler<TRequest> service,
        IJobErrorHandler<TRequest> error,
        IJobSuccessHandler<TRequest> success
    )
    {
        Service = service;
        Error = error;
        Success = success;
    }

    private JobHandler()
    {
        Service = new JobServiceHandlerDefault<TRequest>();
        Error = new JobErrorHandlerDefault<TRequest>();
        Success = new JobSuccessHandlerDefault<TRequest>();
    }

    public IJobServiceHandler<TRequest> Service { get; init; }
    public IJobErrorHandler<TRequest> Error { get; init; }
    public IJobSuccessHandler<TRequest> Success { get; init; }

    public static JobHandler<TRequest> Default => new();
}
