namespace GHM.Job;

public class JobServiceResponse<TRequest>
{
    public JobServiceResponse(DateTime? nextRun, TimeSpan? byInterval)
    {
        NextRun = nextRun;
        ByInterval = byInterval;
    }

    public DateTime? NextRun { get; init; }
    public TimeSpan? ByInterval { get; init; }
}

public class ExecuterResponse<TRequest, TResponse>
{
    public ExecuterResponse(TRequest? request, object? requestId, TResponse? response, Exception? exception)
    {
        Exception = exception;
        Request = request;
        RequestId = requestId;
        Response = response;
    }

    public Exception? Exception { get; }
    public TRequest? Request { get; }
    public object? RequestId { get; }
    public TResponse? Response { get; }
}

public class UpdaterResponse<TRequest, TResponse>
{
    public UpdaterResponse(TRequest? request, object? requestId, TResponse? response, Exception? exception)
    {
        Exception = exception;
        Request = request;
        RequestId = requestId;
        Response = response;
    }

    public Exception? Exception { get; }
    public TRequest? Request { get; }
    public object? RequestId { get; }
    public TResponse? Response { get; }
}

public class RequesterResponse<TRequest>
{
    public RequesterResponse(IEnumerable<RequestData<TRequest>> requests, Exception? exception)
    {
        Exception = exception;
        Requests = requests;
    }

    public Exception? Exception { get; }
    public IEnumerable<RequestData<TRequest>> Requests { get; }
    public object? RequestId { get; }
}

public class RequestData<TRequest>
{
    public RequestData(TRequest? value, object? id)
    {
        Value = value;
        Id = id;
    }

    public object? Id { get; }
    public TRequest? Value { get; }
}
