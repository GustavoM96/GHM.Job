namespace GHM.Job;

public class Job<TRequest, TResponse>
{
    public Job(Func<TRequest, TResponse> executer, Action<TRequest>? updater, JobOptions<TRequest> jobOptions)
    {
        Executer = executer;
        Updater = updater;
        Options = jobOptions;
    }

    public JobOptions<TRequest> Options { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest>? Updater { get; init; }
    public IJobErrorHandler<TRequest> ErrorHandler { get; private set; } = default!;
    public IJobSuccessHandler<TRequest> SuccessHandler { get; private set; } = default!;

    public void SetErrorHandler(IJobErrorHandler<TRequest> handler) => ErrorHandler ??= handler;

    public void SetSuccessHandler(IJobSuccessHandler<TRequest> handler) => SuccessHandler ??= handler;

    protected TResponse? RunExecuter(TRequest request)
    {
        TResponse? response = default;

        try
        {
            response = Executer(request);
            SuccessHandler.AfterExecuter(request, Options.RequestId);
        }
        catch (Exception ex)
        {
            if (Options.OnExecuterError is not null)
            {
                Options.OnExecuterError(ex, request);
            }

            ErrorHandler.OnExecuterError(ex, request, Options.RequestId);
        }

        if (Options.AfterExecuter is not null)
        {
            Options.AfterExecuter(request);
        }
        return response;
    }

    protected void RunUpdater(TRequest request)
    {
        try
        {
            if (Updater is not null)
            {
                Updater(request);
                SuccessHandler.AfterUpdater(request, Options.RequestId);
            }
        }
        catch (Exception ex)
        {
            if (Options.OnUpdaterError is not null)
            {
                Options.OnUpdaterError(ex, request);
            }
            ErrorHandler.OnUpdaterError(ex, request, Options.RequestId);
        }

        if (Options.AfterUpdater is not null)
        {
            Options.AfterUpdater(request);
        }
    }

    protected TResponse? RunRequest(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }
        if (Options.LoggerId is not null)
        {
            Options.RequestId = Options.LoggerId(request);
        }

        SuccessHandler.AfterRequester(request, Options.RequestId);

        var response = RunExecuter(request);
        RunUpdater(request);

        return response;
    }

    protected Task RunAfterWork()
    {
        if (Options.AfterWork is not null)
        {
            Options.AfterWork();
        }

        return Task.CompletedTask;
    }
}

public class JobUniqueRequest<TRequest, TResponse> : Job<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobUniqueRequest(
        Func<TRequest> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<TRequest> Requester { get; init; }

    private TRequest? RunRequester()
    {
        try
        {
            return Requester();
        }
        catch (Exception ex)
        {
            ErrorHandler.OnRequesterError(ex);
            return default;
        }
    }

    public Task DoWork()
    {
        var request = RunRequester();
        RunRequest(request);
        RunAfterWork();

        return Task.CompletedTask;
    }
}

public class JobRequest<TRequest, TResponse> : Job<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobRequest(
        Func<IEnumerable<TRequest>> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<IEnumerable<TRequest>> Requester { get; init; }

    private IEnumerable<TRequest> RunRequester()
    {
        try
        {
            return Requester();
        }
        catch (Exception ex)
        {
            ErrorHandler.OnRequesterError(ex);
            return Enumerable.Empty<TRequest>();
        }
    }

    public Task DoWork()
    {
        var requests = RunRequester();
        foreach (var request in requests)
        {
            RunRequest(request);
        }
        RunAfterWork();

        return Task.CompletedTask;
    }
}
