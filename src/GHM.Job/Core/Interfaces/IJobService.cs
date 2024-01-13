namespace GHM.Job;

public interface IJobService<TRequest>
{
    Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, TimeSpan interval, CancellationToken token = default);
    Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, CancellationToken token = default);
}
