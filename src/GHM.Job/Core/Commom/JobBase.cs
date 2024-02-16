namespace GHM.Job;

public class JobBase<TRequest>
{
    protected object? RequestId { get; set; }
    protected string RequestName { get; } = typeof(TRequest).Name;
    public Action<TRequest>? AfterExecuter { get; init; }
    public Action<TRequest>? AfterUpdater { get; init; }
    public Action? AfterWork { get; init; }
    public Action<Exception, TRequest>? OnExecuterError { get; init; }
    public Action<Exception, TRequest>? OnUpdaterError { get; init; }
    public Func<TRequest, object>? LoggerId { get; init; }

    protected IJobHandler Handler { get; private set; } = default!;

    public void SetHandler(IJobHandler handler) => Handler ??= handler;
}
