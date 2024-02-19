using Moq;

namespace GHM.Job.Test;

public class JobHandlerTests
{
    private readonly Mock<IJobHandler<string>> _jobHandler = new();
    private readonly JobServiceHandlerTest _jobServiceHandler = new();

    private readonly IJobService<string> _jobService;

    public JobHandlerTests()
    {
        _jobService = new JobService<string>(
            new UtcAddingHoursTimeZoneStrategy(-5),
            JobHandler<string>.Default.Job,
            _jobServiceHandler
        );
    }

    [Fact]
    public async Task Test_SuccessHandler_WhenRun_Successfully_ShouldRun_DoWork()
    {
        // Arrange
        var result = "processing";
        var request = " => data";
        var id = 12345;

        string Requester() => request;
        string Executer(string data) => result += data + " => Executer";
        void Updater(string request, string? response) => result += " => Updater";
        void AfterWork() => result += " => AfterWork";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        object LoggerId(string data) => id;

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterWork: AfterWork, afterExecuter: AfterExecuter, loggerId: LoggerId)
        );

        job.SetHandler(JobHandler<string>.Default.Job);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);

        // _jobHandler.Verify(handler => handler.HandleRequester(It.IsAny<Func<Task<RequesterResponse<string>>>>()));
        // _jobHandler.Verify(handler => handler.HandleExecuter(It.IsAny<Func<Task<ExecuterResponse<string, string>>>>()));
        // _jobHandler.Verify(handler => handler.HandleUpdater(It.IsAny<Func<Task<UpdaterResponse<string>>>>()));
    }

    [Fact]
    public async Task Test_ServiceHandler_AtUniqueRunMode_ShouldRun_HandleWork()
    {
        // Arrange
        var result = "processing";
        var request = " => data";
        var id = 12345;

        string Requester() => request;
        string Executer(string data) => result += data + " => Executer";
        void Updater(string request, string? response) => result += " => Updater";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        object LoggerId(string data) => id;

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterExecuter: AfterExecuter, loggerId: LoggerId)
        );

        async Task UniqueRunMode() => await _jobService.ExecuteAsync(job, default);
        await UniqueRunMode();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater", result);
        Assert.True(_jobServiceHandler.HasDoneWork);
    }

    [Fact]
    public async Task Test_ServiceHandler_AtIntervalMode_ShouldRun_HandleWork()
    {
        // Arrange
        var result = "processing";
        var request = " => data";
        var id = 12345;

        var source = new CancellationTokenSource();
        var token = source.Token;

        string Requester() => request;
        string Executer(string data) => result += data + " => Executer";
        void Updater(string request, string? response) => result += " => Updater";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        object LoggerId(string data) => id;
        void AfterWork()
        {
            result += " => AfterWork";
            if (_jobServiceHandler.HasDoneWork)
            {
                source.Cancel();
            }
        }

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterWork: AfterWork, afterExecuter: AfterExecuter, loggerId: LoggerId)
        );

        async Task IntervalMode() => await _jobService.ExecuteAsync(job, TimeSpan.FromMilliseconds(1), token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(IntervalMode);
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);
    }

    [Fact]
    public async Task Test_ServiceHandler_AtCronMode_ShouldRun_HandleWork()
    {
        // Arrange
        var result = "processing";
        var request = " => data";
        var id = 12345;

        var source = new CancellationTokenSource();
        var token = source.Token;

        string Requester() => request;
        string Executer(string data) => result += data + " => Executer";
        void Updater(string request, string? response) => result += " => Updater";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        object LoggerId(string data) => id;
        void AfterWork()
        {
            if (_jobServiceHandler.HasDoneWork)
            {
                source.Cancel();
            }
        }

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterWork: AfterWork, afterExecuter: AfterExecuter, loggerId: LoggerId)
        );

        async Task CronMode() => await _jobService.ExecuteAsync(job, "* * * * *", token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(CronMode);
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater", result);
    }
}

public class JobServiceHandlerTest : IJobServiceHandler<string>
{
    public bool HasDoneWork { get; private set; } = false;

    public async Task<JobServiceResponse<string>> HandleWork(Func<Task<JobServiceResponse<string>>> runWork)
    {
        HasDoneWork = true;
        return await runWork();
    }
}
