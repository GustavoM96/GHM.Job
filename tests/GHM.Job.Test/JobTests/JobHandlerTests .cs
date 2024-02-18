using Moq;

namespace GHM.Job.Test;

public class JobHandlerTests
{
    private readonly Mock<IJobErrorHandler<string>> _jobErrorHandler = new();
    private readonly Mock<IJobSuccessHandler<string>> _jobSuccessHandler = new();
    private readonly JobServiceHandlerTest _jobServiceHandler = new();

    private readonly IJobService<string> _jobService;

    public JobHandlerTests()
    {
        _jobService = new JobService<string>(
            new UtcAddingHoursTimeZoneStrategy(-5),
            JobHandler<string>.Default.Error,
            JobHandler<string>.Default.Success,
            _jobServiceHandler
        );
    }

    [Fact]
    public async Task Test_ErrorHandler_WhenThrow_ExceptionAtExecuter_ShouldRun_OnExecuterErrorAndOnUpdaterError()
    {
        // Arrange
        var request = " => data";
        var result = "processing";
        var id = 12345;

        var executerException = new Exception($" => Error at Executer");
        var updaterException = new Exception($" => Error at Updater");

        string Requester() => request;
        string Executer(string data) => throw executerException;
        void Updater(string data) => throw updaterException;
        void OnExecuterError(Exception exception, string data) => result += data + exception.Message;
        void OnUpdaterError(Exception exception, string data) => result += data + exception.Message;
        object LoggerId(string data) => id;

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(onExecuterError: OnExecuterError, onUpdaterError: OnUpdaterError, loggerId: LoggerId)
        );

        job.SetErrorHandler(_jobErrorHandler.Object);
        job.SetSuccessHandler(_jobSuccessHandler.Object);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Error at Executer => data => Error at Updater", result);
        _jobErrorHandler.Verify(handler => handler.OnExecuterError(executerException, request, id));
        _jobErrorHandler.Verify(handler => handler.OnUpdaterError(updaterException, request, id));

        _jobSuccessHandler.Verify(handler => handler.AfterUpdater(request, id), Times.Never);
        _jobSuccessHandler.Verify(handler => handler.AfterExecuter(request, id), Times.Never);
    }

    [Fact]
    public async Task Test_ErrorHandler_WhenThrow_Exception_ShouldRun_OnRequesterError()
    {
        // Arrange
        var id = 12345;
        var exception = new Exception($"Error at processing");

        string Requester() => throw exception;
        string Executer(string data) => throw exception;
        object LoggerId(string data) => id;

        // Act
        var job = JobFactory.Create(requester: Requester, executer: Executer, jobOptions: new(loggerId: LoggerId));

        job.SetErrorHandler(_jobErrorHandler.Object);
        job.SetSuccessHandler(_jobSuccessHandler.Object);
        await job.DoWork();

        // Assert
        _jobErrorHandler.Verify(handler => handler.OnRequesterError(exception));
        _jobSuccessHandler.VerifyNoOtherCalls();
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
        void Updater(string data) => result += " => Updater";
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

        job.SetErrorHandler(_jobErrorHandler.Object);
        job.SetSuccessHandler(_jobSuccessHandler.Object);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);

        _jobSuccessHandler.Verify(handler => handler.AfterRequester(request, id));
        _jobSuccessHandler.Verify(handler => handler.AfterExecuter(request, id));
        _jobSuccessHandler.Verify(handler => handler.AfterUpdater(request, id));
        _jobErrorHandler.VerifyNoOtherCalls();
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
        void Updater(string data) => result += " => Updater";
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
        void Updater(string data) => result += " => Updater";
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
        void Updater(string data) => result += " => Updater";
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
