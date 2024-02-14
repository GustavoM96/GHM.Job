namespace GHM.Job;

public interface IJobService<TRequest>
{
    Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, TimeSpan interval, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, string cron, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(JobAsync<TRequest, TResponse> job, TimeSpan interval, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(JobAsync<TRequest, TResponse> job, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(JobAsync<TRequest, TResponse> job, string cron, CancellationToken token = default);
}
