namespace GHM.Job;

public interface IJobHandler<TRequest>
{
    IJobServiceHandler<TRequest> Service { get; init; }
    IJobErrorHandler<TRequest> Error { get; init; }
    IJobSuccessHandler<TRequest> Success { get; init; }
}

public interface IJobErrorHandler<TRequest>
{
    void OnRequesterError(Exception ex);

    void OnExecuterError(Exception ex, TRequest request, object? requestId);

    void OnUpdaterError(Exception ex, TRequest request, object? requestId);
}

public interface IJobServiceHandler<TRequest>
{
    Task<JobServiceResponse> HandleWork(Func<Task<JobServiceResponse>> runWork);
}

public interface IJobSuccessHandler<TRequest>
{
    void AfterRequester(TRequest request, object? requestId);

    void AfterExecuter(TRequest request, object? requestId);

    void AfterUpdater(TRequest request, object? requestId);
}
