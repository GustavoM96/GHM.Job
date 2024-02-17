namespace GHM.Job;

public class JobServiceResponse<TRequest>
{
    public JobServiceResponse(DateTime? nextRun, TimeSpan? byInterval, IEnumerable<JobResponse<TRequest>> jobResponses)
    {
        NextRun = nextRun;
        ByInterval = byInterval;
        JobResponses = jobResponses;
    }

    public DateTime? NextRun { get; init; }
    public TimeSpan? ByInterval { get; init; }
    public IEnumerable<JobResponse<TRequest>> JobResponses { get; init; }
}

public class JobResponse<TRequest>
{
    public JobResponse(TRequest? request, object? requestId, object? response)
    {
        Request = request;
        RequestId = requestId;
        Response = response;
    }

    public TRequest? Request { get; init; }
    public object? RequestId { get; init; }
    public object? Response { get; init; }
}
