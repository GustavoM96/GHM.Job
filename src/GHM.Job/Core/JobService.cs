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
        IJob<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        job.SetHandler(_jobHandler);

        while (!token.IsCancellationRequested)
        {
            await Task.Run(job.DoWork, token);
            _jobHandler.Service.HandleAfterWork(_timeZoneStrategy.Now.Add(interval), _requestName);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        await Task.Run(job.DoWork, token);
        _jobHandler.Service.HandleAfterWork(null, _requestName);
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
                await Task.Run(job.DoWork, token);
                nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);
                _jobHandler.Service.HandleAfterWork(nextOccurrence, _requestName);
            }

            await Task.Delay(1000, token);
        }
    }
}
