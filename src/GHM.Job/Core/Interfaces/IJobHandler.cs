namespace GHM.Job;

public interface IJobErrorHandler
{
    void HandleOnRequester(Exception ex, string requestName);

    void HandleOnExcuter(Exception ex, string requestName, object? requestId);

    void HandleOnUpdater(Exception ex, string requestName, object? requestId);
}

public interface IJobServiceHandler
{
    void HandleOnAfterWork(DateTime? nextRun, string requestName);
}

public interface IJobSuccessHandler
{
    void HandleOnRequester(string requestName, object? requestId);

    void HandleOnExecuter(string requestName, object? requestId);

    void HandleOnUpdater(string requestName, object? requestId);
}
