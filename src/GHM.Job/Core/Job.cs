namespace GHM.Job;

public class Job<TRequest, TResponse>
{
    public Job(
        Func<IEnumerable<TRequest>>? requester,
        Func<TRequest>? requesterUnique,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? afterExecuter,
        Action<TRequest>? afterUpdater,
        Action? afterWork,
        Action<TRequest>? updater,
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
    public Func<IEnumerable<TRequest>>? Requester { get; init; }
    public Func<TRequest>? RequesterUnique { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest>? Updater { get; init; }
    public JobHandler Handler { get; private set; } = default!;

    public void SetHandler(JobHandler handler) => Handler ??= handler;

    private TResponse? RunExecuter(TRequest? request)
    {
        try
        {
            var response = Executer(request!);
            Handler.Success.HandleOnExecuter(Options.RequestName, Options.RequestId);

            return response;
        }
        catch (Exception ex)
        {
            if (Options.OnExecuterError is not null)
            {
                Options.OnExecuterError(ex, request!);
            }

            Handler.Error.HandleOnExcuter(ex, Options.RequestName, Options.RequestId);
            return default;
        }
        finally
        {
            if (Options.AfterExecuter is not null)
            {
                Options.AfterExecuter(request!);
            }
        }
    }

    private void RunUpdater(TRequest? request)
    {
        try
        {
            if (Updater is not null)
            {
                Updater(request!);
                Handler.Success.HandleOnUpdater(Options.RequestName, Options.RequestId);
            }
        }
        catch (Exception ex)
        {
            if (Options.OnUpdaterError is not null)
            {
                Options.OnUpdaterError(ex, request!);
            }
            Handler.Error.HandleOnUpdater(ex, Options.RequestName, Options.RequestId);
        }
        finally
        {
            if (Options.AfterUpdater is not null)
            {
                Options.AfterUpdater(request!);
            }
        }
    }

    private IEnumerable<TRequest> RunRequester()
    {
        try
        {
            return Requester!();
        }
        catch (Exception ex)
        {
            Handler.Error.HandleOnRequester(ex, Options.RequestName);
            return Enumerable.Empty<TRequest>();
        }
    }

    private TRequest? RunRequesterUnique()
    {
        try
        {
            return RequesterUnique!();
        }
        catch (Exception ex)
        {
            Handler.Error.HandleOnRequester(ex, Options.RequestName);
            return default;
        }
    }

    private TResponse? RunRequest(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }
        if (Options.LoggerId is not null)
        {
            Options.RequestId = Options.LoggerId(request);
        }

        Handler.Success.HandleOnRequester(Options.RequestName, Options.RequestId);

        var response = RunExecuter(request);
        RunUpdater(request);

        return response;
    }

    private void RunAfterWork()
    {
        if (Options.AfterWork is not null)
        {
            Options.AfterWork();
        }
    }

    public void DoWork()
    {
        if (RequesterUnique is not null)
        {
            var request = RunRequesterUnique();
            RunRequest(request);
            RunAfterWork();
            return;
        }

        if (Requester is not null)
        {
            var requests = RunRequester();
            foreach (var request in requests)
            {
                RunRequest(request);
            }
            RunAfterWork();
        }
    }
}

public static class Job
{
    public static Job<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<IEnumerable<TRequest>> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater = null,
        Action? afterWork = null,
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        return new Job<TRequest, TResponse>(
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

    public static Job<TRequest, TResponse> Create<TRequest, TResponse>(
        Func<TRequest> requesterUnique,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater = null,
        Action? afterWork = null,
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        return new Job<TRequest, TResponse>(
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
