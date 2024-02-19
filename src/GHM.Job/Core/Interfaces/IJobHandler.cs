namespace GHM.Job;

public interface IJobServiceHandler<TRequest>
{
    Task<JobServiceResponse<TRequest>> HandleWork(Func<Task<JobServiceResponse<TRequest>>> runWork);
}

public interface IJobHandler<TRequest>
{
    Task<RequesterResponse<TRequest>> HandleRequester(Func<Task<RequesterResponse<TRequest>>> requester);

    Task<ExecuterResponse<TRequest, TResponse>> HandleExecuter<TResponse>(
        Func<Task<ExecuterResponse<TRequest, TResponse>>> requester
    );

    Task<UpdaterResponse<TRequest, TResponse>> HandleUpdater<TResponse>(
        Func<Task<UpdaterResponse<TRequest, TResponse>>> updater
    );
}
