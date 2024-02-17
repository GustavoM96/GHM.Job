namespace GHM.Job;

public interface IJobService<TRequest>
{
    Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, TimeSpan interval, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, string cron, CancellationToken token = default);
}
