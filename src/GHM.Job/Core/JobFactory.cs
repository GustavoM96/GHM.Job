namespace GHM.Job;

public static class JobFactory
{
    public static IJob<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<IEnumerable<TRequest>> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater = null,
        JobOptions<TRequest>? jobOptions = null
    )
    {
        return new JobRequest<TRequest, TResponse>(requester, executer, updater, jobOptions ?? new());
    }

    public static IJob<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<TRequest> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater = null,
        JobOptions<TRequest>? jobOptions = null
    )
    {
        return new JobUniqueRequest<TRequest, TResponse>(requester, executer, updater, jobOptions ?? new());
    }
}

public static class JobAsyncFactory
{
    public static IJob<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<Task<IEnumerable<TRequest>>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater = null,
        JobOptions<TRequest>? jobOptions = null
    )
    {
        return new JobRequestAsync<TRequest, TResponse>(requester, executer, updater, jobOptions ?? new());
    }

    public static IJob<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<Task<TRequest>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater = null,
        JobOptions<TRequest>? jobOptions = null
    )
    {
        return new JobUniqueRequestAsync<TRequest, TResponse>(requester, executer, updater, jobOptions ?? new());
    }
}

public class JobOptionsFactory
{
    public static JobOptions<TRequest> Create<TRequest>(
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action? afterWork = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        return new JobOptions<TRequest>(afterExecuter, afterUpdater, afterWork, onExecuterError, onUpdaterError, loggerId);
    }
}
