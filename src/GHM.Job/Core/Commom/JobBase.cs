namespace GHM.Job;

public class JobOptions<TRequest>
{
    public object? RequestId { get; set; }
    public string RequestName { get; } = typeof(TRequest).Name;
    public Action<TRequest>? AfterExecuter { get; init; }
    public Action<TRequest>? AfterUpdater { get; init; }
    public Action? AfterWork { get; init; }
    public Action<Exception, TRequest>? OnExecuterError { get; init; }
    public Action<Exception, TRequest>? OnUpdaterError { get; init; }
    public Func<TRequest, object>? LoggerId { get; init; }
}
