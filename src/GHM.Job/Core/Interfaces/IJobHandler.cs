namespace GHM.Job;

public interface IJobErrorHandler
{
    void OnRequesterError(Exception ex, string requestName);

    void OnExecuterError(Exception ex, string requestName, object? requestId);

    void OnUpdaterError(Exception ex, string requestName, object? requestId);
}

public interface IJobServiceHandler
{
    void HandleAfterWork(DateTime? nextRun, string requestName);
}

public interface IJobSuccessHandler
{
    void AfterRequester(string requestName, object? requestId);

    void AfterExecuter(string requestName, object? requestId);

    void AfterUpdater(string requestName, object? requestId);
}
