namespace GHM.Job;

public interface IJob<TRequest, TResponse>
{
    void SetHandler(JobHandler handler);
    Task DoWork();
}
