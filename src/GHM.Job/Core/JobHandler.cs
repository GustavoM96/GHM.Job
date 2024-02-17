namespace GHM.Job;

public class JobErrorHandlerDefault : IJobErrorHandler
{
    public void OnRequesterError(Exception ex, string requestName) { }

    public void OnExecuterError(Exception ex, string requestName, object? requestId) { }

    public void OnUpdaterError(Exception ex, string requestName, object? requestId) { }
}

public class JobSuccessHandlerDefault : IJobSuccessHandler
{
    public void AfterRequester(string requestName, object? requestId) { }

    public void AfterExecuter(string requestName, object? requestId) { }

    public void AfterUpdater(string requestName, object? requestId) { }
}

public class JobServiceHandlerDefault : IJobServiceHandler
{
    public async Task<JobServiceResponse> HandleWork<TRequest>(Func<Task<JobServiceResponse>> runWork) => await runWork();
}

public class JobHandler
{
    public JobHandler(IJobServiceHandler? service = null, IJobErrorHandler? error = null, IJobSuccessHandler? success = null)
    {
        Service = service ?? new JobServiceHandlerDefault();
        Error = error ?? new JobErrorHandlerDefault();
        Success = success ?? new JobSuccessHandlerDefault();
    }

    private JobHandler()
    {
        Service = new JobServiceHandlerDefault();
        Error = new JobErrorHandlerDefault();
        Success = new JobSuccessHandlerDefault();
    }

    public IJobServiceHandler Service { get; init; }
    public IJobErrorHandler Error { get; init; }
    public IJobSuccessHandler Success { get; init; }

    public static JobHandler Default => new();
}
