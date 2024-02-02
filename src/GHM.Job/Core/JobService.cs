namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    public async Task ExecuteAsync<TResponse>(
        Job<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Run(job.DoWork, token);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, CancellationToken token = default)
    {
        await Task.Run(job.DoWork, token);
    }

    public async Task ExecuteAsync<TResponse>(
        JobAsync<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Run(job.DoWork, token);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(JobAsync<TRequest, TResponse> job, CancellationToken token = default)
    {
        await Task.Run(job.DoWork, token);
    }
}
