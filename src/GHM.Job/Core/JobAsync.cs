namespace GHM.Job;

public class JobAsync<TRequest, TResponse>
{
    public JobAsync(
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, TResponse?, Task>? updater,
        JobOptions<TRequest> jobOptions
    )
    {
        Executer = executer;
        Updater = updater;
        Options = jobOptions;
    }

    public JobOptions<TRequest> Options { get; init; }

    public Func<TRequest, Task<TResponse>> Executer { get; init; }
    public Func<TRequest, TResponse?, Task>? Updater { get; init; }
    public IJobHandler<TRequest> Handler { get; private set; } = default!;

    public void SetHandler(IJobHandler<TRequest> handler) => Handler ??= handler;

    protected async Task<TResponse?> RunExecuter(TRequest request)
    {
        TResponse? response = default;
        async Task<ExecuterResponse<TRequest, TResponse>> DoExecuter()
        {
            Exception? exception = default;
            try
            {
                response = await Executer(request);
                if (Options.AfterExecuter is not null)
                {
                    Options.AfterExecuter(request);
                }
            }
            catch (Exception ex)
            {
                exception = ex;

                if (Options.OnExecuterError is not null)
                {
                    Options.OnExecuterError(ex, request);
                }
            }

            var jobResponse = new ExecuterResponse<TRequest, TResponse>(
                request,
                Options.GetId(request),
                response,
                exception
            );
            return jobResponse;
        }

        await Handler.HandleExecuter(DoExecuter);
        return response;
    }

    protected async Task RunUpdater(TRequest request, TResponse? response)
    {
        async Task<UpdaterResponse<TRequest, TResponse>> DoUpdater()
        {
            Exception? exception = default;
            try
            {
                if (Updater is not null)
                {
                    await Updater(request, response);
                }
                if (Options.AfterUpdater is not null)
                {
                    Options.AfterUpdater(request);
                }
            }
            catch (Exception ex)
            {
                if (Options.OnUpdaterError is not null)
                {
                    Options.OnUpdaterError(ex, request);
                }
            }

            var jobResponse = new UpdaterResponse<TRequest, TResponse>(request, Options.GetId(request), response, exception);
            return jobResponse;
        }

        await Handler.HandleUpdater(DoUpdater);
    }

    protected async Task<TResponse?> RunRequest(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }

        var response = await RunExecuter(request);
        await RunUpdater(request, response);

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
        Func<TRequest, TResponse?, Task>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<Task<TRequest>> Requester { get; init; }

    private async Task<TRequest?> RunRequester()
    {
        TRequest? request = default;

        async Task<RequesterResponse<TRequest>> DoRequester()
        {
            Exception? exception = default;
            try
            {
                request = await Requester();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var requestData = new RequestData<TRequest>[1] { new(request, Options.GetId(request)) };
            return new RequesterResponse<TRequest>(requestData, exception);
        }

        await Handler.HandleRequester(DoRequester);
        return request;
    }

    public async Task DoWork()
    {
        var request = await RunRequester();
        await RunRequest(request);
        RunAfterWork();
    }
}

public class JobRequestAsync<TRequest, TResponse> : JobAsync<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobRequestAsync(
        Func<Task<IEnumerable<TRequest>>> requester,
        Func<TRequest, Task<TResponse>> executer,
        Func<TRequest, TResponse?, Task>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<Task<IEnumerable<TRequest>>> Requester { get; init; }

    private async Task<IEnumerable<TRequest>> RunRequester()
    {
        IEnumerable<TRequest> request = Enumerable.Empty<TRequest>();

        async Task<RequesterResponse<TRequest>> DoRequester()
        {
            Exception? exception = default;
            try
            {
                request = await Requester();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var requestData = request.Select(req => new RequestData<TRequest>(req, Options.GetId(req)));

            return new RequesterResponse<TRequest>(requestData, exception);
        }

        await Handler.HandleRequester(DoRequester);
        return request;
    }

    public async Task DoWork()
    {
        var requests = (await RunRequester()).ToList();

        foreach (var request in requests)
        {
            await RunRequest(request);
        }

        RunAfterWork();
    }
}
