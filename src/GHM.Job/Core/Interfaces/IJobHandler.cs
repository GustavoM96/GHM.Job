namespace GHM.Job;

public interface IJobErrorHandler
{
    void OnRequesterError(Exception ex, string requestName);

    void OnExecuterError(Exception ex, string requestName, object? requestId);

    void OnUpdaterError(Exception ex, string requestName, object? requestId);
}

public interface IJobServiceHandler
{
    Task<JobServiceResponse> HandleWork<TRequest>(Func<Task<JobServiceResponse>> runWork);
}

public interface IJobSuccessHandler
{
    void AfterRequester(string requestName, object? requestId);

    void AfterExecuter(string requestName, object? requestId);

    void AfterUpdater(string requestName, object? requestId);
}
