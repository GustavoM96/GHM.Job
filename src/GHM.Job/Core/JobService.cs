using NCrontab;

namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    private readonly ITimeZoneStrategy _timeZoneStrategy;
    private readonly IJobHandler<TRequest> _jobHandler;
    private readonly IJobServiceHandler<TRequest> _serviceHandler;

    public JobService(
        ITimeZoneStrategy timeZoneStrategy,
        IJobHandler<TRequest> jobHandler,
        IJobServiceHandler<TRequest> serviceHandler
    )
    {
        _timeZoneStrategy = timeZoneStrategy;
        _jobHandler = jobHandler;
        _serviceHandler = serviceHandler;
    }

    public async Task ExecuteAsync<TResponse>(
        IJob<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        job.SetHandler(_jobHandler);

        while (!token.IsCancellationRequested)
        {
            async Task<JobServiceResponse<TRequest>> Work()
            {
                await Task.Run(job.DoWork, token);
                return new JobServiceResponse<TRequest>(_timeZoneStrategy.Now.Add(interval), interval);
            }

            await _serviceHandler.HandleWork(Work);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        async Task<JobServiceResponse<TRequest>> Work()
        {
            await Task.Run(job.DoWork, token);
            return new JobServiceResponse<TRequest>(null, null);
        }

        await _serviceHandler.HandleWork(Work);
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, string cron, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        var crontabSchedule = CrontabSchedule.Parse(cron);
        var nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);

        while (!token.IsCancellationRequested)
        {
            if (_timeZoneStrategy.Now > nextOccurrence)
            {
                async Task<JobServiceResponse<TRequest>> Work()
                {
                    await Task.Run(job.DoWork, token);

                    nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);
                    return new JobServiceResponse<TRequest>(nextOccurrence, null);
                }

                await _serviceHandler.HandleWork(Work);
            }

            await Task.Delay(1000, token);
        }
    }
}
