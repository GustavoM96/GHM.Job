namespace GHM.Job;

public class Job<TRequest, TResponse>
{
    public Job(Func<TRequest, TResponse> executer, Action<TRequest, TResponse?>? updater, JobOptions<TRequest> jobOptions)
    {
        Executer = executer;
        Updater = updater;
        Options = jobOptions;
    }

    public JobOptions<TRequest> Options { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest, TResponse?>? Updater { get; init; }
    public IJobHandler<TRequest> Handler { get; private set; } = default!;

    public void SetHandler(IJobHandler<TRequest> handler) => Handler ??= handler;

    protected async Task<TResponse?> RunExecuter(TRequest request)
    {
        TResponse? response = default;

        Task<ExecuterResponse<TResponse>> DoExecuter(TRequest req)
        {
            Exception? exception = default;
            try
            {
                Handler.HandleBeforeExecuter(req, Options.GetId(req));

                response = Executer(req);
                if (Options.AfterExecuter is not null)
                {
                    Options.AfterExecuter(req);
                }
            }
            catch (Exception ex)
            {
                exception = ex;

                if (Options.OnExecuterError is not null)
                {
                    Options.OnExecuterError(ex, req);
                }
            }

            var jobResponse = new ExecuterResponse<TResponse>(response, exception);
            return Task.FromResult(jobResponse);
        }

        await Handler.HandleExecuter(DoExecuter, request, Options.GetId(request));
        return response;
    }

    protected async Task RunUpdater(TRequest request, TResponse? response)
    {
        Task<UpdaterResponse<TResponse>> DoUpdater(TRequest req)
        {
            Exception? exception = default;
            try
            {
                if (Updater is not null)
                {
                    Updater(req, response);
                }
                if (Options.AfterUpdater is not null)
                {
                    Options.AfterUpdater(req);
                }
            }
            catch (Exception ex)
            {
                if (Options.OnUpdaterError is not null)
                {
                    Options.OnUpdaterError(ex, req);
                }
            }

            var jobResponse = new UpdaterResponse<TResponse>(response, exception);
            return Task.FromResult(jobResponse);
        }

        await Handler.HandleUpdater(DoUpdater, request, Options.GetId(request));
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

public class JobUniqueRequest<TRequest, TResponse> : Job<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobUniqueRequest(
        Func<TRequest> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest, TResponse?>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<TRequest> Requester { get; init; }

    private async Task<TRequest?> RunRequester()
    {
        TRequest? request = default;

        Task<RequesterResponse<TRequest>> DoRequester()
        {
            Exception? exception = default;
            try
            {
                request = Requester();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var requestData = new RequestData<TRequest>[1] { new(request, Options.GetId(request)) };
            var jobResponse = new RequesterResponse<TRequest>(requestData, exception);
            return Task.FromResult(jobResponse);
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

public class JobRequest<TRequest, TResponse> : Job<TRequest, TResponse>, IJob<TRequest, TResponse>
{
    public JobRequest(
        Func<IEnumerable<TRequest>> requester,
        Func<TRequest, TResponse> executer,
        Action<TRequest, TResponse?>? updater,
        JobOptions<TRequest> jobOptions
    )
        : base(executer, updater, jobOptions)
    {
        Requester = requester;
    }

    public Func<IEnumerable<TRequest>> Requester { get; init; }

    private async Task<IEnumerable<TRequest>> RunRequester()
    {
        IEnumerable<TRequest> request = Enumerable.Empty<TRequest>();

        Task<RequesterResponse<TRequest>> DoRequester()
        {
            Exception? exception = default;
            try
            {
                request = Requester();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var requestData = request.Select(req => new RequestData<TRequest>(req, Options.GetId(req)));

            var jobResponse = new RequesterResponse<TRequest>(requestData, exception);
            return Task.FromResult(jobResponse);
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
