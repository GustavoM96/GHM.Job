namespace GHM.Job.Test;

public class JobAsyncServiceTests
{
    private readonly IJobService<string> _jobService = new JobService<string>(
        new UtcAddingHoursTimeZoneStrategy(-3),
        JobHandler<string>.Default.Error,
        JobHandler<string>.Default.Success,
        JobHandler<string>.Default.Service
    );

    [Fact]
    public async Task Test_ExecuteAsync_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => Task.FromResult(result += data + " => Executer");

        // Act
        var job = JobAsyncFactory.Create(requester: Requester, executer: Executer);
        await _jobService.ExecuteAsync(job);

        // Assert
        Assert.Equal("processing => data => Executer", result);
    }

    [Fact]
    public async Task Test_ExecuteAsync_When_CancelToken_ShouldNotRun()
    {
        // Arrange
        var result = "processing";

        var source = new CancellationTokenSource();
        var token = source.Token;

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => Task.FromResult(result += data + " => Executer");
        source.Cancel();

        // Act
        var job = JobAsyncFactory.Create(requester: Requester, executer: Executer);
        async Task Run() => await _jobService.ExecuteAsync(job, token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(Run);
    }

    [Fact]
    public async Task Test_ExecuteAsync_When_SetInterval_ShouldRun_UntilCancel()
    {
        // Arrange
        var result = "processing";

        var source = new CancellationTokenSource();
        var token = source.Token;
        var executeCounter = 0;

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => Task.FromResult(result += data + $" => Executer{executeCounter}");
        void AfterExecuter(string data) => executeCounter++;
        void AfterWork()
        {
            if (executeCounter == 2)
                source.Cancel();
        }

        // Act
        var job = JobAsyncFactory.Create(
            requester: Requester,
            executer: Executer,
            jobOptions: new(afterExecuter: AfterExecuter, afterWork: AfterWork)
        );
        async Task Run() => await _jobService.ExecuteAsync(job, TimeSpan.FromSeconds(0.1), token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(Run);
        Assert.Equal("processing => data => Executer0 => data => Executer1", result);
    }

    [Fact]
    public async Task Test_ExecuteAsync_When_SetCron_ShouldRun_UntilCancel()
    {
        // Arrange
        var result = "processing";

        var source = new CancellationTokenSource();
        var token = source.Token;
        var executeCounter = 0;

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => Task.FromResult(result += data + $" => Executer{executeCounter}");
        void AfterExecuter(string data) => executeCounter++;
        void AfterWork()
        {
            if (executeCounter == 1)
                source.Cancel();
        }

        // Act
        var job = JobAsyncFactory.Create(
            requester: Requester,
            executer: Executer,
            jobOptions: new(afterExecuter: AfterExecuter, afterWork: AfterWork)
        );
        async Task Run() => await _jobService.ExecuteAsync(job, "* * * * *", token);

        // Assert
        // Assertsd

        await Assert.ThrowsAsync<TaskCanceledException>(Run);
        Assert.Equal("processing => data => Executer0", result);
    }
}
