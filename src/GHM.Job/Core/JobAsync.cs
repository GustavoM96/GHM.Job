namespace GHM.Job;

public class JobAsync<TRequest, TResponse> : IJob<TRequest, TResponse>
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
        Updater = updater;
        Options = new JobOptions<TRequest>()
        {
            AfterExecuter = afterExecuter,
            AfterUpdater = afterUpdater,
            AfterWork = afterWork,
            OnExecuterError = onExecuterError,
            OnUpdaterError = onUpdaterError,
            LoggerId = loggerId
        };
    }

    public JobOptions<TRequest> Options { get; init; }

    public Func<Task<IEnumerable<TRequest>>>? Requester { get; init; }
    public Func<Task<TRequest>>? RequesterUnique { get; init; }
    public Func<TRequest, Task<TResponse>> Executer { get; init; }
    public Func<TRequest, Task>? Updater { get; init; }
    public JobHandler Handler { get; private set; } = default!;

    public void SetHandler(JobHandler handler) => Handler ??= handler;

    private async Task<TResponse?> RunExecuter(TRequest request)
    {
        try
        {
            var response = await Executer(request);
            Handler.Success.AfterExecuter(Options.RequestName, Options.RequestId);

            return response;
        }
        catch (Exception ex)
        {
            if (Options.OnExecuterError is not null)
            {
                Options.OnExecuterError(ex, request);
            }

            Handler.Error.OnExecuterError(ex, Options.RequestName, Options.RequestId);
            return default;
        }
        finally
        {
            if (Options.AfterExecuter is not null)
            {
                Options.AfterExecuter(request);
            }
        }
    }

    private async Task RunUpdater(TRequest request)
    {
        try
        {
            if (Updater is not null)
            {
                await Updater(request);
                Handler.Success.AfterUpdater(Options.RequestName, Options.RequestId);
            }
        }
        catch (Exception ex)
        {
            if (Options.OnUpdaterError is not null)
            {
                Options.OnUpdaterError(ex, request);
            }
            Handler.Error.OnUpdaterError(ex, Options.RequestName, Options.RequestId);
        }
        finally
        {
            if (Options.AfterUpdater is not null)
            {
                Options.AfterUpdater(request);
            }
        }
    }

    private async Task<IEnumerable<TRequest>> RunRequester()
    {
        try
        {
            return await Requester!();
        }
        catch (Exception ex)
        {
            Handler.Error.OnRequesterError(ex, Options.RequestName);
            return Enumerable.Empty<TRequest>();
        }
    }

    private async Task<TRequest?> RunRequesterUnique()
    {
        try
        {
            return await RequesterUnique!();
        }
        catch (Exception ex)
        {
            Handler.Error.OnRequesterError(ex, Options.RequestName);
            return default;
        }
    }

    private async Task<TResponse?> RunRequest(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }

        if (Options.LoggerId is not null)
        {
            Options.RequestId = Options.LoggerId(request);
        }

        Handler.Success.AfterRequester(Options.RequestName, Options.RequestId);
        var response = await RunExecuter(request);
        await RunUpdater(request);

        return response;
    }

    private void RunAfterWork()
    {
        if (Options.AfterWork is not null)
        {
            Options.AfterWork();
        }
    }

    public async Task DoWork()
    {
        if (RequesterUnique is not null)
        {
            var request = await RunRequesterUnique();
            await RunRequest(request);
            RunAfterWork();
            return;
        }

        if (Requester is not null)
        {
            var requests = await RunRequester();
            foreach (var request in requests)
            {
                await RunRequest(request);
            }
            RunAfterWork();
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

    public static IJob<TRequest, TResponse> Create<TRequest, TResponse>(
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
