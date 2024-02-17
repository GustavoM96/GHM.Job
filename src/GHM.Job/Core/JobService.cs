using NCrontab;

namespace GHM.Job;

public class JobService<TRequest> : IJobService<TRequest>
{
    private readonly ITimeZoneStrategy _timeZoneStrategy;
    private readonly IJobErrorHandler<TRequest> _errorHandler;
    private readonly IJobSuccessHandler<TRequest> _successHandler;
    private readonly IJobServiceHandler<TRequest> _serviceHandler;

    public JobService(
        ITimeZoneStrategy timeZoneStrategy,
        IJobErrorHandler<TRequest> errorHandler,
        IJobSuccessHandler<TRequest> successHandler,
        IJobServiceHandler<TRequest> serviceHandler
    )
    {
        _timeZoneStrategy = timeZoneStrategy;
        _errorHandler = errorHandler;
        _successHandler = successHandler;
        _serviceHandler = serviceHandler;
    }

    public async Task ExecuteAsync<TResponse>(
        IJob<TRequest, TResponse> job,
        TimeSpan interval,
        CancellationToken token = default
    )
    {
        job.SetErrorHandler(_errorHandler);
        job.SetSuccessHandler(_successHandler);

        while (!token.IsCancellationRequested)
        {
            async Task<JobServiceResponse> Work()
            {
                await Task.Run(job.DoWork, token);
                return new JobServiceResponse(_timeZoneStrategy.Now.Add(interval));
            }

            await _serviceHandler.HandleWork(Work);
            await Task.Delay(interval, token);
        }
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, CancellationToken token = default)
    {
        job.SetErrorHandler(_errorHandler);
        job.SetSuccessHandler(_successHandler);

        async Task<JobServiceResponse> Work()
        {
            await Task.Run(job.DoWork, token);
            return new JobServiceResponse(null);
        }

        await _serviceHandler.HandleWork(Work);
    }

    public async Task ExecuteAsync<TResponse>(IJob<TRequest, TResponse> job, string cron, CancellationToken token = default)
    {
        job.SetErrorHandler(_errorHandler);
        job.SetSuccessHandler(_successHandler);

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

                await _serviceHandler.HandleWork(Work);
            }

            await Task.Delay(1000, token);
        }
    }
}
