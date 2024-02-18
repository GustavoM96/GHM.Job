namespace GHM.Job.Test;

public class JobServiceTests
{
    private readonly IJobService<string> _jobService = new JobService<string>(
        new UtcAddingHoursTimeZoneStrategy(-5),
        JobHandler<string>.Default.Error,
        JobHandler<string>.Default.Success,
        JobHandler<string>.Default.Service
    );

    [Fact]
    public async Task Test_ExecuteAsync_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";

        // Act
        var job = JobFactory.Create(requester: Requester, executer: Executer);
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
        var job = JobFactory.Create(requester: Requester, executer: Executer);
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
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            jobOptions: new(afterExecuter: AfterExecuter, afterWork: AfterWork)
        );
        async Task Run() => await _jobService.ExecuteAsync(job, TimeSpan.FromMilliseconds(5), token);

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
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            jobOptions: new(afterExecuter: AfterExecuter, afterWork: AfterWork)
        );
        async Task Run() => await _jobService.ExecuteAsync(job, "* * * * *", token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(Run);
        Assert.Equal("processing => data => Executer0", result);
    }

    [Fact]
    public void Test_TimeZoneStrategy()
    {
        // Arrange
        var jobServiceNow = new NowTimeZoneStrategy();
        var jobServiceUtc = new UtcTimeZoneStrategy();
        var jobServiceutcAdd2Hours = new UtcAddingHoursTimeZoneStrategy(2);

        // Act

        // Assert
        Assert.True(DatesAreAlmostEqual(DateTime.Now, jobServiceNow.Now));
        Assert.True(DatesAreAlmostEqual(DateTime.UtcNow, jobServiceUtc.Now));
        Assert.True(DatesAreAlmostEqual(DateTime.UtcNow.AddHours(2), jobServiceutcAdd2Hours.Now));
    }

    private static bool DatesAreAlmostEqual(DateTime dateTimeBase, DateTime dateTimeinput)
    {
        return dateTimeBase.AddSeconds(1) > dateTimeinput && dateTimeBase.AddSeconds(-1) < dateTimeinput;
    }
}
