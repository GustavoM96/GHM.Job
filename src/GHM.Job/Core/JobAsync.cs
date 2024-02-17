namespace GHM.Job;

public class JobAsync<TRequest, TResponse>
{
    public JobAsync(Func<TRequest, Task<TResponse>> executer, Func<TRequest, Task>? updater, JobOptions<TRequest> jobOptions)
    {
        Executer = executer;
        Updater = updater;
        Options = jobOptions;
    }

    public JobOptions<TRequest> Options { get; init; }

    public Func<TRequest, Task<TResponse>> Executer { get; init; }
    public Func<TRequest, Task>? Updater { get; init; }
    public IJobErrorHandler<TRequest> ErrorHandler { get; private set; } = default!;
    public IJobSuccessHandler<TRequest> SuccessHandler { get; private set; } = default!;

    public void SetErrorHandler(IJobErrorHandler<TRequest> handler) => ErrorHandler ??= handler;

    public void SetSuccessHandler(IJobSuccessHandler<TRequest> handler) => SuccessHandler ??= handler;

    protected async Task<TResponse?> RunExecuter(TRequest request)
    {
        try
        {
            var response = await Executer(request);
            SuccessHandler.AfterExecuter(request, Options.RequestId);

            return response;
        }
        catch (Exception ex)
        {
            if (Options.OnExecuterError is not null)
            {
                Options.OnExecuterError(ex, request);
            }

            ErrorHandler.OnExecuterError(ex, request, Options.RequestId);
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

    protected async Task RunUpdater(TRequest request)
    {
        try
        {
            if (Updater is not null)
            {
                await Updater(request);
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
        finally
        {
            if (Options.AfterUpdater is not null)
            {
                Options.AfterUpdater(request);
            }
        }
    }

    protected async Task<TResponse?> RunRequest(TRequest? request)
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
        var response = await RunExecuter(request);
        await RunUpdater(request);

        return response;
    }

    protected void RunAfterWork()
    {
        if (Options.AfterWork is not null)
        {
            Options.AfterWork();
        }
    }
}

public class JobUniqueRequestAsync<TRequest, TResponse> : JobAsync<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobUniqueRequestAsync(
        Func<Task<TRequest>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<Task<TRequest>> Requester { get; init; }

    private async Task<TRequest?> RunRequester()
    {
        try
        {
            return await Requester();
        }
        catch (Exception ex)
        {
            ErrorHandler.OnRequesterError(ex);
            return default;
        }
    }

    public async Task DoWork()
    {
        var request = await RunRequester();
        await RunRequest(request);
        RunAfterWork();
        return;
    }
}

public class JobRequestAsync<TRequest, TResponse> : JobAsync<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobRequestAsync(
        Func<Task<IEnumerable<TRequest>>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, Task>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<Task<IEnumerable<TRequest>>> Requester { get; init; }

    private async Task<IEnumerable<TRequest>> RunRequester()
    {
        try
        {
            return await Requester();
        }
        catch (Exception ex)
        {
            ErrorHandler.OnRequesterError(ex);
            return Enumerable.Empty<TRequest>();
        }
    }

    public async Task DoWork()
    {
        var requests = await RunRequester();
        foreach (var request in requests)
        {
            await RunRequest(request);
        }
        RunAfterWork();
    }
}
