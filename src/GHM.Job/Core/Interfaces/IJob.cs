namespace GHM.Job;

public interface IJob<TRequest, TResponse>
{
    void SetErrorHandler(IJobErrorHandler<TRequest> handler);
    void SetSuccessHandler(IJobSuccessHandler<TRequest> handler);
    Task DoWork();
}
