using NCrontab;

namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    private readonly ITimeZoneStrategy _timeZoneStrategy;
    private readonly JobHandler _jobHandler;
    private readonly string _requestName = typeof(TRequest).Name;

    public JobService(ITimeZoneStrategy timeZoneStrategy, JobHandler jobHandler)
    {
        _timeZoneStrategy = timeZoneStrategy;
        _jobHandler = jobHandler;
    }

    public async Task ExecuteAsync<TResponse>(
        Job<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        job.SetHandler(_jobHandler);

        while (!token.IsCancellationRequested)
        {
            await Task.Run(job.DoWork, token);
            _jobHandler.Service.HandleOnAfterWork(_timeZoneStrategy.Now.Add(interval), _requestName);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        await Task.Run(job.DoWork, token);
        _jobHandler.Service.HandleOnAfterWork(null, _requestName);
    }

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, string cron, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        var crontabSchedule = CrontabSchedule.Parse(cron);
        var nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);

        while (!token.IsCancellationRequested)
        {
            if (_timeZoneStrategy.Now > nextOccurrence)
            {
                await Task.Run(job.DoWork, token);
                nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);
                _jobHandler.Service.HandleOnAfterWork(nextOccurrence, _requestName);
            }

            await Task.Delay(1000, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(
        JobAsync<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        job.SetHandler(_jobHandler);

        while (!token.IsCancellationRequested)
        {
            await Task.Run(job.DoWork, token);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(JobAsync<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);
        await Task.Run(job.DoWork, token);
    }

    public async Task ExecuteAsync<TResponse>(
        JobAsync<TRequest, TResponse> job,
        string cron,
        CancellationToken token = default
    )
    {
        job.SetHandler(_jobHandler);

        var crontabSchedule = CrontabSchedule.Parse(cron);
        var nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);

        while (!token.IsCancellationRequested)
        {
            if (_timeZoneStrategy.Now > nextOccurrence)
            {
                await Task.Run(job.DoWork, token);
                nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);
            }

            await Task.Delay(1000, token);
        }
    }
}
