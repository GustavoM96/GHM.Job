namespace GHM.Job;

public interface IJob<TRequest, TResponse>
{
    void SetHandler(IJobHandler<TRequest> handler);
    Task DoWork();
}
