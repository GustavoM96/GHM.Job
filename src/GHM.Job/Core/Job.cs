namespace GHM.Job;

public class Job<TRequest, TResponse> : IJob<TRequest, TResponse>
{
    public Job(
        Func<IEnumerable<TRequest>>? requester,
        Func<TRequest>? requesterUnique,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater,
        JobOptions<TRequest> jobOptions
    )
    {
        Requester = requester;
        RequesterUnique = requesterUnique;
        Executer = executer;
        Updater = updater;
        Options = jobOptions;
    }

    public JobOptions<TRequest> Options { get; init; }
    public Func<IEnumerable<TRequest>>? Requester { get; init; }
    public Func<TRequest>? RequesterUnique { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest>? Updater { get; init; }
    public JobHandler Handler { get; private set; } = default!;

    public void SetHandler(JobHandler handler) => Handler ??= handler;

    private TResponse? RunExecuter(TRequest request)
    {
        TResponse? response = default;

        try
        {
            response = Executer(request);
            Handler.Success.AfterExecuter(Options.RequestName, Options.RequestId);
        }
        catch (Exception ex)
        {
            if (Options.OnExecuterError is not null)
            {
                Options.OnExecuterError(ex, request);
            }

            Handler.Error.OnExecuterError(ex, Options.RequestName, Options.RequestId);
        }

        if (Options.AfterExecuter is not null)
        {
            Options.AfterExecuter(request);
        }
        return response;
    }

    private void RunUpdater(TRequest request)
    {
        try
        {
            if (Updater is not null)
            {
                Updater(request);
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

        if (Options.AfterUpdater is not null)
        {
            Options.AfterUpdater(request);
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
            Handler.Error.OnRequesterError(ex, Options.RequestName);
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
            Handler.Error.OnRequesterError(ex, Options.RequestName);
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

        Handler.Success.AfterRequester(Options.RequestName, Options.RequestId);

        var response = RunExecuter(request);
        RunUpdater(request);

        return response;
    }

    private Task RunAfterWork()
    {
        if (Options.AfterWork is not null)
        {
            Options.AfterWork();
        }

        return Task.CompletedTask;
    }

    public Task DoWork()
    {
        if (RequesterUnique is not null)
        {
            var request = RunRequesterUnique();
            RunRequest(request);
            return RunAfterWork();
        }

        if (Requester is not null)
        {
            var requests = RunRequester();
            foreach (var request in requests)
            {
                RunRequest(request);
            }
        }

        return RunAfterWork();
    }
}
