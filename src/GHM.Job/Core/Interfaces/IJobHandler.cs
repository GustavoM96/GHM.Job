namespace GHM.Job;

public interface IJobServiceHandler<TRequest>
{
    Task<JobServiceResponse<TRequest>> HandleWork(Func<Task<JobServiceResponse<TRequest>>> runWork);
}

public interface IJobHandler<TRequest>
{
    void HandleBeforeExecuter(TRequest request, object? requestId);

    Task<RequesterResponse<TRequest>> HandleRequester(Func<Task<RequesterResponse<TRequest>>> requester);

    Task<ExecuterResponse<TResponse>> HandleExecuter<TResponse>(
        Func<TRequest, Task<ExecuterResponse<TResponse>>> requester,
        TRequest request,
        object? requestId
    );

    Task<UpdaterResponse<TResponse>> HandleUpdater<TResponse>(
        Func<TRequest, Task<UpdaterResponse<TResponse>>> updater,
        TRequest request,
        object? requestId
    );
}
