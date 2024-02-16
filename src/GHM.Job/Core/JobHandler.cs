namespace GHM.Job;

public class JobErrorHandlerDefault : IJobErrorHandler
{
    public void HandleOnRequester(Exception ex, string requestName) { }

    public void HandleOnExcuter(Exception ex, string requestName, object? requestId) { }

    public void HandleOnUpdater(Exception ex, string requestName, object? requestId) { }
}

public class JobSuccessHandlerDefault : IJobSuccessHandler
{
    public void HandleOnRequester(string requestName, object? requestId) { }

    public void HandleOnExecuter(string requestName, object? requestId) { }

    public void HandleOnUpdater(string requestName, object? requestId) { }
}

public class JobServiceHandlerDefault : IJobServiceHandler
{
    public void HandleOnAfterWork(DateTime? nextRun, string requestName) { }
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
