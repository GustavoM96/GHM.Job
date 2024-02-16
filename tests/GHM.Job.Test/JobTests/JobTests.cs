namespace GHM.Job.Test;

public class JobTests
{
    [Fact]
    public void Test_DoWork_When_PassAllFunctions_ShouldRun_Parameters()
    {
        // Arrange
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

        job.SetHandler(JobHandler.Default);
        job.DoWork();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);
    }

    [Fact]
    public void Test_DoWork_WhenPassOnlyExecuter_ShouldRun_Executer()
    {
        // Arrange
        var result = "processing";

        string[] Requester() => new string[2] { " => data1", " => data2" };
        string Executer(string data) => result += data + " => Executer";

        // Act
        var job = Job.Create(requester: Requester, executer: Executer);

        job.SetHandler(JobHandler.Default);
        job.DoWork();

        // Assert
        Assert.Equal("processing => data1 => Executer => data2 => Executer", result);
    }

    [Fact]
    public void Test_DoWork_WhenThrowException_ShouldRun_OnError()
    {
        // Arrange
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => throw new Exception($"{data} => Error at Executer");
        void Updater(string data) => throw new Exception($"{data} => Error at Updater");
        void OnExecuterError(Exception exception, string data) => result += exception.Message;
        void OnUpdaterError(Exception exception, string data) => result += exception.Message;

        // Act
        var job = Job.Create(
            requesterUnique: Requester,
            executer: Executer,
            updater: Updater,
            onExecuterError: OnExecuterError,
            onUpdaterError: OnUpdaterError
        );
        job.SetHandler(JobHandler.Default);
        job.DoWork();

        // Assert
        Assert.Equal("processing => data => Error at Executer => data => Error at Updater", result);
    }
}
