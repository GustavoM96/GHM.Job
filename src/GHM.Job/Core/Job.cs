﻿namespace GHM.Job;

public class Job<TRequest, TResponse>
{
    public Job(
        Func<IEnumerable<TRequest>>? requester,
        Func<TRequest>? requesterUnique,
        Func<TRequest, TResponse> executer,
        Action<TRequest>? afterExecuter,
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
        AfterWork = afterWork;
        Updater = updater;
        OnExecuterError = onExecuterError;
        OnUpdaterError = onUpdaterError;
        LoggerId = loggerId;
    }

    private object? Id { get; set; }
    private readonly string _requestName = typeof(TRequest).Name;
    public Func<IEnumerable<TRequest>>? Requester { get; init; }
    public Func<TRequest>? RequesterUnique { get; init; }
    public Func<TRequest, TResponse> Executer { get; init; }
    public Action<TRequest>? AfterExecuter { get; init; }
    public Action? AfterWork { get; init; }
    public Action<TRequest>? Updater { get; init; }
    public Action<Exception, TRequest>? OnExecuterError { get; init; }
    public Action<Exception, TRequest>? OnUpdaterError { get; init; }
    public Func<TRequest, object>? LoggerId { get; init; }

    private TResponse? RunExecuter(TRequest? request)
    {
        if (request is null)
        {
            return default;
        }

        try
        {
            var response = Executer(request);
            if (AfterExecuter is not null)
            {
                AfterExecuter(request);
            }

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
    }

    private void RunUpdater(TRequest? request)
    {
        if (request is null)
        {
            return;
        }

        try
        {
            if (Updater is not null)
            {
                Updater(request);
            }
        }
        catch (Exception ex)
        {
            if (OnUpdaterError is not null)
            {
                OnUpdaterError(ex, request);
            }
        }
    }

    private IEnumerable<TRequest> RunRequester()
    {
        try
        {
            return Requester!();
        }
        catch (Exception)
        {
            return Enumerable.Empty<TRequest>();
        }
    }

    private TResponse? RunRequest(TRequest? request)
    {
        var response = RunExecuter(request);
        RunUpdater(request);

        if (AfterWork is not null)
        {
            AfterWork();
        }

        return response;
    }

    private TRequest? RunRequesterUnique()
    {
        try
        {
            return RequesterUnique!();
        }
        catch (Exception)
        {
            return default;
        }
    }

    public void DoWork()
    {
        if (RequesterUnique is not null)
        {
            var request = RunRequesterUnique();
            RunRequest(request);
            return;
        }
        if (Requester is not null)
        {
            var requests = RunRequester();
            foreach (var request in requests)
            {
                RunRequest(request);
            }
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
            afterWork,
            updater,
            onExecuterError,
            onUpdaterError,
            loggerId
        );
    }
}
