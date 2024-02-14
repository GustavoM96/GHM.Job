namespace GHM.Job.Test;

public class JobServiceTests
{
    private readonly IJobService<string> _jobService = new JobService<string>();

    [Fact]
    public async Task Test_ExecuteAsync_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";

        // Act
        var job = Job.Create(requesterUnique: Requester, executer: Executer);
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

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";
        source.Cancel();

        // Act
        var job = Job.Create(requesterUnique: Requester, executer: Executer);
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

        string Requester() => " => data";
        string Executer(string data) => result += data + $" => Executer{executeCounter}";
        void AfterExecuter(string data) => executeCounter++;
        void AfterWork()
        {
            if (executeCounter == 2)
                source.Cancel();
        }

        // Act
        var job = Job.Create(
            requesterUnique: Requester,
            executer: Executer,
            afterExecuter: AfterExecuter,
            afterWork: AfterWork
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

        string Requester() => " => data";
        string Executer(string data) => result += data + $" => Executer{executeCounter}";
        void AfterExecuter(string data) => executeCounter++;
        void AfterWork()
        {
            if (executeCounter == 1)
                source.Cancel();
        }

        // Act
        var job = Job.Create(
            requesterUnique: Requester,
            executer: Executer,
            afterExecuter: AfterExecuter,
            afterWork: AfterWork
        );
        async Task Run() => await _jobService.ExecuteAsync(job, "* * * * *", token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(Run);
        Assert.Equal("processing => data => Executer0", result);
    }
}
