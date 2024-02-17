namespace GHM.Job;

public class JobServiceResponse
{
    public JobServiceResponse(DateTime? nextRun)
    {
        NextRun = nextRun;
    }

    public DateTime? NextRun { get; init; }
}
