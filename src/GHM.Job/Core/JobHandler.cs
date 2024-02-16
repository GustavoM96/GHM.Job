namespace GHM.Job;

public interface IJobHandler
{
    public void HandleOnRequesterError(Exception ex, string requestName, object? requestId) { }

    public void HandleOnExcuterError(Exception ex, string requestName, object? requestId) { }

    public void HandleOnUpdaterError(Exception ex, string requestName, object? requestId) { }

    void HandleOnAfterWork() { }
}

public class JobHandler : IJobHandler
{
    public void HandleOnRequesterError(Exception ex, string requestName, object? requestId) { }

    public void HandleOnExcuterError(Exception ex, string requestName, object? requestId) { }

    public void HandleOnUpdaterError(Exception ex, string requestName, object? requestId) { }

    public void HandleOnAfterWork() { }
}
