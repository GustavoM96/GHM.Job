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
        Task Updater(string data) => Task.FromResult(result += " => Updater");
        void AfterWork() => result += " => AfterWork";
        void AfterExecuter(string data) => result += " => AfterExecuter";
        string LoggerId(string data) => data;

        // Act
        var job = JobAsync.Create(
            requesterUnique: Requester,
            executer: Executer,
            updater: Updater,
            afterWork: AfterWork,
            afterExecuter: AfterExecuter,
            loggerId: LoggerId
        );
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
        var job = JobAsync.Create(requester: Requester, executer: Executer);
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
        Task Updater(string data) => throw new Exception($"{data} => Error at Updater");
        void OnExecuterError(Exception exception, string data) => result += exception.Message;
        void OnUpdaterError(Exception exception, string data) => result += exception.Message;

        // Act
        var job = JobAsync.Create(
            requesterUnique: Requester,
            executer: Executer,
            updater: Updater,
            onExecuterError: OnExecuterError,
            onUpdaterError: OnUpdaterError
        );
        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Error at Executer => data => Error at Updater", result);
    }
}