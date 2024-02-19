namespace GHM.Job.Test;

public class JobAsyncTests
{
    [Fact]
    public async Task Test_DoWork_When_PassAllFunctions_ShouldRun_Parameters()
    {
        // Arrange
        var result = "processing";

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => Task.FromResult(result += data + " => Executer");
        Task Updater(string data, string? response) => Task.FromResult(result += " => Updater");
        void AfterWork() => result += " => AfterWork";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        string LoggerId(string data) => data;

        // Act

        var job = JobAsyncFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterExecuter: AfterExecuter, afterWork: AfterWork, loggerId: LoggerId)
        );

        job.SetHandler(JobHandler<string>.Default.Job);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);
    }

    [Fact]
    public async Task Test_DoWork_WhenPassOnlyExecuter_ShouldRun_Executer()
    {
        // Arrange
        var result = "processing";

        Task<IEnumerable<string>> Requester() => Task.FromResult(new string[2] { " => data1", " => data2" }.AsEnumerable());
        Task<string> Executer(string data) => Task.FromResult(result += data + " => Executer");

        // Act
        var job = JobAsyncFactory.Create(requester: Requester, executer: Executer);

        job.SetHandler(JobHandler<string>.Default.Job);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data1 => Executer => data2 => Executer", result);
    }

    [Fact]
    public async Task Test_DoWork_WhenThrowException_ShouldRun_OnError()
    {
        // Arrange
        var result = "processing";

        Task<string> Requester() => Task.FromResult(" => data");
        Task<string> Executer(string data) => throw new Exception($"{data} => Error at Executer");
        Task Updater(string data, string? response) => throw new Exception($"{data} => Error at Updater");
        void OnExecuterError(Exception exception, string data) => result += exception.Message;
        void OnUpdaterError(Exception exception, string data) => result += exception.Message;

        // Act
        var job = JobAsyncFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(onExecuterError: OnExecuterError, onUpdaterError: OnUpdaterError)
        );

        job.SetHandler(JobHandler<string>.Default.Job);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Error at Executer => data => Error at Updater", result);
    }
}
