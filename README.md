<p align="center">
<img src="logo.png" alt="logo" width="200px"/>
</p>

<h1 align="center"> GHM.Job </h1>

GHM.Job is a nuget package aims to run jobs in separates ways(Get Request, Run Request and Update Request).

## Install Package

.NET CLI

```sh
dotnet add package GHM.Job
```

Package Manager

```sh
NuGet\Install-Package GHM.Job
```

## IServiceCollectionExtensions

To add scoped interface `IJobService<>` to implementate `JobService<>` , call extension method to your serviceCollection.

```csharp
using GHM.Job.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
service.AddGhmJob();
```

If you want to set `TimeZoneStrategy` to set a DateTime.Now for executeAsync with cron string `* * * * *`, pass one of these strategies at calling AddGhmJob(ITimeZoneStrategy timeZoneStrategy).

- NowTimeZoneStrategy()
- UtcTimeZoneStrategy()
- UtcAddingHoursTimeZoneStrategy(int addhours)

```csharp
using GHM.Job.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var strategy = new UtcAddingHoursTimeZoneStrategy(-3) // -3Hours UTC
service.AddGhmJob(strategy);
```

```csharp
public class NowTimeZoneStrategy : ITimeZoneStrategy
{
    public DateTime Now => DateTime.Now;
}

public class UtcTimeZoneStrategy : ITimeZoneStrategy
{
    public DateTime Now => DateTime.UtcNow;
}

public class UtcAddingHoursTimeZoneStrategy : ITimeZoneStrategy
{
    private readonly int _addhours;

    public UtcAddingHoursTimeZoneStrategy(int addhours)
    {
        _addhours = addhours;
    }

    public DateTime Now => DateTime.UtcNow.AddHours(_addhours);
}
```

## Example

### To run a Job

The Job Run the Sequence:

- Get Requests Unique or List
- Execute Request
- After Execute Request
- On error Execute Request
- Update Request
- After Update Request
- On error Update Request
- After Work Request

```csharp
using GHM.Job;

public class MyBackgroundService : BackgroundService
{
    private readonly IJobService<string> _jobService = new JobService<string>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";
        void Updater(string data) => result += " => Updater";
        void AfterWork() => result += " => AfterWork";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        string LoggerId(string data) => data;

        // Act
        var job = Job.Create(
            requesterUnique: Requester,
            executer: Executer,
            updater: Updater,
            afterWork: AfterWork,
            afterExecuter: AfterExecuter,
            loggerId: LoggerId
        );

        // Setting delay: 1 second to the next job running.
        await _jobService.ExecuteAsync(job, TimeSpan.FromSeconds(1), stoppingToken);

        // Running 1 time.
        await _jobService.ExecuteAsync(job, stoppingToken);

        // Setting cron: At minute 5 to job running.
        // More informations about cron. https://crontab.guru
        await _jobService.ExecuteAsync(job, "5 * * * *" ,stoppingToken);

        // result = "processing => data => Executer => AfterExecuter => Updater => AfterWork"
    }
}
```

You can run your job async.

```csharp
using GHM.Job;

public class MyBackgroundService : BackgroundService
{
    private readonly IJobService<string> _jobService = new JobService<string>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var result = "processing";

        Task<string> RequesterAsync() => Task.FromResult(" => data");
        Task<string> ExecuterAsync(string data) => Task.FromResult(result += data + " => Executer");
        Task UpdaterAsync(string data) => Task.FromResult(result += " => Updater");
        void AfterWork() => result += " => AfterWork";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        string LoggerId(string data) => data;

        // Act
        var job = JobAsync.Create(
            requesterUnique: RequesterAsync,
            executer: ExecuterAsync,
            updater: UpdaterAsync,
            afterWork: AfterWork,
            afterExecuter: AfterExecuter,
            loggerId: LoggerId
        );

        // Setting delay: 1 second to the next job running.
        await _jobService.ExecuteAsync(job, TimeSpan.FromSeconds(1), stoppingToken);

        // Running 1 time.
        await _jobService.ExecuteAsync(job, stoppingToken);

        // result = "processing => data => Executer => AfterExecuter => Updater => AfterWork"
    }
}
```

## Classes

### IJobService

It is a interface implemented by `JobService<TRequest>`

```csharp
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

    public async Task ExecuteAsync<TResponse>(Job<TRequest, TResponse> job, string cron, CancellationToken token = default)
    {
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
}
```

## Star

if you enjoy, don't forget the ⭐ and install the package 😊.
