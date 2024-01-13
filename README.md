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
- property to log Id

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
}
```

## Star

if you enjoy, don't forget the ⭐ and install the package 😊.
