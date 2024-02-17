using NCrontab;

namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    private readonly ITimeZoneStrategy _timeZoneStrategy;
    private readonly JobHandler _jobHandler;

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
            async Task<JobServiceResponse> Work()
            {
                await Task.Run(job.DoWork, token);
                return new JobServiceResponse(_timeZoneStrategy.Now.Add(interval));
            }

            await _jobHandler.Service.HandleWork<TRequest>(Work);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetHandler(_jobHandler);

        async Task<JobServiceResponse> Work()
        {
            await Task.Run(job.DoWork, token);
            return new JobServiceResponse(null);
        }

        await _jobHandler.Service.HandleWork<TRequest>(Work);
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
                async Task<JobServiceResponse> Work()
                {
                    await Task.Run(job.DoWork, token);

                    nextOccurrence = crontabSchedule.GetNextOccurrence(_timeZoneStrategy.Now);
                    return new JobServiceResponse(nextOccurrence);
                }

                var result = await _jobHandler.Service.HandleWork<TRequest>(Work);
            }

            await Task.Delay(1000, token);
        }
    }
}
