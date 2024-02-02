namespace GHM.Job;

public class JobAsync<TRequest, TResponse>
{
    public JobAsync(
        Func<Task<IEnumerable<TRequest>>>? requester,
        Func<Task<TRequest>>? requesterUnique,
        Func<TRequest, Task<TResponse>> executer,
        Action<TRequest>? afterExecuter,
        Action<TRequest>? afterUpdater,
        Action? afterWork,
        Func<TRequest, Task>? updater,
        Action<Exception, TRequest>? onExecuterError,
        Action<Exception, TRequest>? onUpdaterError,
        Func<TRequest, object>? loggerId
    )
    {
        Requester = requester;
        RequesterUnique = requesterUnique;
        Executer = executer;
        AfterExecuter = afterExecuter;
        AfterUpdater = afterUpdater;
        AfterWork = afterWork;
        Updater = updater;
        OnExecuterError = onExecuterError;
        OnUpdaterError = onUpdaterError;
        LoggerId = loggerId;
    }

    private object? Id { get; set; }
    private readonly string _requestName = typeof(TRequest).Name;
    public Func<Task<IEnumerable<TRequest>>>? Requester { get; init; }
    public Func<Task<TRequest>>? RequesterUnique { get; init; }
    public Func<TRequest, Task<TResponse>> Executer { get; init; }
    public Action<TRequest>? AfterExecuter { get; init; }
    public Action<TRequest>? AfterUpdater { get; init; }
    public Action? AfterWork { get; init; }
    public Func<TRequest, Task>? Updater { get; init; }
    public Action<Exception, TRequest>? OnExecuterError { get; init; }
    public Action<Exception, TRequest>? OnUpdaterError { get; init; }
    public Func<TRequest, object>? LoggerId { get; init; }

    private async Task<TResponse?> RunExecuter(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }

        try
        {
            var response = await Executer(request);

            return response;
        }
        catch (Exception ex)
        {
            if (OnExecuterError is not null)
            {
                OnExecuterError(ex, request);
            }

            return default;
        }
        finally
        {
            if (AfterExecuter is not null)
            {
                AfterExecuter(request);
            }
        }
    }

    private async Task RunUpdater(TRequest? request)
    {
        if (request is null)
        {
            return;
        }

        try
        {
            if (Updater is not null)
            {
                await Updater(request);
            }
        }
        catch (Exception ex)
        {
            if (OnUpdaterError is not null)
            {
                OnUpdaterError(ex, request);
            }
        }
        finally
        {
            if (AfterUpdater is not null)
            {
                AfterUpdater(request);
            }
        }
    }

    private async Task<IEnumerable<TRequest>> RunRequester()
    {
        try
        {
            return await Requester!();
        }
        catch (Exception)
        {
            return Enumerable.Empty<TRequest>();
        }
    }

    private async Task<TResponse?> RunRequest(TRequest? request)
    {
        var response = await RunExecuter(request);
        await RunUpdater(request);

        return response;
    }

    private async Task<TRequest?> RunRequesterUnique()
    {
        try
        {
            return await RequesterUnique!();
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task DoWork()
    {
        if (RequesterUnique is not null)
        {
            var request = await RunRequesterUnique();
            await RunRequest(request);
        }

        if (Requester is not null)
        {
            var requests = await RunRequester();
            foreach (var request in requests)
            {
                await RunRequest(request);
            }
        }

        if (AfterWork is not null)
        {
            AfterWork();
        }
    }
}

public static class JobAsync
{
    public static JobAsync<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<Task<IEnumerable<TRequest>>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater = null,
        Action? afterWork = null,
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        return new JobAsync<TRequest, TResponse>(
            requester,
            null,
            executer,
            afterExecuter,
            afterUpdater,
            afterWork,
            updater,
            onExecuterError,
            onUpdaterError,
            loggerId
        );
    }

    public static JobAsync<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<Task<TRequest>> requesterUnique,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater = null,
        Action? afterWork = null,
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        return new JobAsync<TRequest, TResponse>(
            null,
            requesterUnique,
            executer,
            afterExecuter,
            afterUpdater,
            afterWork,
            updater,
            onExecuterError,
            onUpdaterError,
            loggerId
        );
    }
}
