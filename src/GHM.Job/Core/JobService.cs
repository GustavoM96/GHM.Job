﻿using NCrontab;

namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    private readonly ITimeZoneStrategy _timeZoneStrategy;
    private readonly IJobHandler _jobHandler;

    public JobService(ITimeZoneStrategy timeZoneStrategy, IJobHandler jobHandler)
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
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);
        await Task.Run(job.DoWork, token);
    }

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, string cron, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);
        var crontabSchedule = CrontabSchedule.Parse(cron);
        var nextOccurrence = crontabSchedule.GetNextOccurrence(DateTime.Now);

        while (!token.IsCancellationRequested)
        {
            if (DateTime.Now > nextOccurrence)
            {
                await Task.Run(job.DoWork, token);
                nextOccurrence = crontabSchedule.GetNextOccurrence(DateTime.Now);
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

    public async Task ExecuteAsync<TResponse>(
        JobAsync<TRequest, TResponse> job,
        string cron,
        CancellationToken token = default
    )
    {
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
