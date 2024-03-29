﻿namespace GHM.Job;

public class JobOptions<TRequest>
{
    public JobOptions(
        Action<TRequest>? afterExecuter = null,
        Action<TRequest>? afterUpdater = null,
        Action? afterWork = null,
        Action<Exception, TRequest>? onExecuterError = null,
        Action<Exception, TRequest>? onUpdaterError = null,
        Func<TRequest, object>? loggerId = null
    )
    {
        AfterExecuter = afterExecuter;
        AfterUpdater = afterUpdater;
        AfterWork = afterWork;
        OnExecuterError = onExecuterError;
        OnUpdaterError = onUpdaterError;
        LoggerId = loggerId;
    }

    public object? GetId(TRequest? request)
    {
        return LoggerId is null || request is null ? null : LoggerId(request);
    }

    public Action<TRequest>? AfterExecuter { get; init; }
    public Action<TRequest>? AfterUpdater { get; init; }
    public Action? AfterWork { get; init; }
    public Action<Exception, TRequest>? OnExecuterError { get; init; }
    public Action<Exception, TRequest>? OnUpdaterError { get; init; }
    private Func<TRequest, object>? LoggerId { get; init; }
}
