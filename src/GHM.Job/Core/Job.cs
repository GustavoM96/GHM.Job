namespace GHM.Job;

public class Job<TRequest, TResponse> : JobBase<TRequest>
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
        AfterExecuter = afterExecuter;
        AfterUpdater = afterUpdater;
        AfterWork = afterWork;
        Updater = updater;
        OnExecuterError = onExecuterError;
        OnUpdaterError = onUpdaterError;
        LoggerId = loggerId;
    }

    public Func<IEnumerable<TRequest>>? Requester { get; init; }
    public Func<TRequest>? RequesterUnique { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest>? Updater { get; init; }

    private TResponse? RunExecuter(TRequest? request)
    {
        try
        {
            var response = Executer(request!);

            return response;
        }
        catch (Exception ex)
        {
            if (OnExecuterError is not null)
            {
                OnExecuterError(ex, request!);
            }

            Handler.HandleOnExcuterError(ex, RequestName, RequestId);
            return default;
        }
        finally
        {
            if (AfterExecuter is not null)
            {
                AfterExecuter(request!);
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
            }
        }
        catch (Exception ex)
        {
            if (OnUpdaterError is not null)
            {
                OnUpdaterError(ex, request!);
            }
            Handler.HandleOnUpdaterError(ex, RequestName, RequestId);
        }
        finally
        {
            if (AfterUpdater is not null)
            {
                AfterUpdater(request!);
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
            Handler.HandleOnRequesterError(ex, RequestName, RequestId);
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
            Handler.HandleOnRequesterError(ex, RequestName, RequestId);
            return default;
        }
    }

    private TResponse? RunRequest(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }

        var response = RunExecuter(request);
        RunUpdater(request);

        return response;
    }

    private void RunAfterWork()
    {
        if (AfterWork is not null)
        {
            AfterWork();
            Handler.HandleOnAfterWork();
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
