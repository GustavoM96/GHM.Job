namespace GHM.Job.Test;

public class JobTests
{
    [Fact]
    public async Task Test_DoWork_When_PassAllFunctions_ShouldRun_Parameters()
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
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(afterWork: AfterWork, afterExecuter: AfterExecuter, loggerId: LoggerId)
        );

        job.SetErrorHandler(JobHandler<string>.Default.Error);
        job.SetSuccessHandler(JobHandler<string>.Default.Success);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Executer => AfterExecuter => Updater => AfterWork", result);
    }

    [Fact]
    public async Task Test_DoWork_WhenPassOnlyExecuter_ShouldRun_Executer()
    {
        // Arrange
        var result = "processing";

        string[] Requester() => new string[2] { " => data1", " => data2" };
        string Executer(string data) => result += data + " => Executer";

        // Act
        var job = JobFactory.Create(requester: Requester, executer: Executer);

        job.SetErrorHandler(JobHandler<string>.Default.Error);
        job.SetSuccessHandler(JobHandler<string>.Default.Success);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data1 => Executer => data2 => Executer", result);
    }

    [Fact]
    public async Task Test_DoWork_WhenThrowException_ShouldRun_OnError()
    {
        // Arrange
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => throw new Exception($"{data} => Error at Executer");
        void Updater(string data) => throw new Exception($"{data} => Error at Updater");
        void OnExecuterError(Exception exception, string data) => result += exception.Message;
        void OnUpdaterError(Exception exception, string data) => result += exception.Message;

        // Act
        var job = JobFactory.Create(
            requester: Requester,
            executer: Executer,
            updater: Updater,
            jobOptions: new(onExecuterError: OnExecuterError, onUpdaterError: OnUpdaterError)
        );

        job.SetErrorHandler(JobHandler<string>.Default.Error);
        job.SetSuccessHandler(JobHandler<string>.Default.Success);

        await job.DoWork();

        // Assert
        Assert.Equal("processing => data => Error at Executer => data => Error at Updater", result);
    }
}
