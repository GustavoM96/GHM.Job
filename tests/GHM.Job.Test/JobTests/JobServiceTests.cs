namespace GHM.Job.Test;

public class JobServiceTests
{
    private readonly IJobService<string> _jobService = new JobService<string>();

    [Fact]
    public void Test_ExecuteAsync_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";

        // Act
        var job = Job.Create(requesterUnique: Requester, executer: Executer);
        _jobService.ExecuteAsync(job);

        // Assert
        Assert.Equal("processing => data => Executer", result);
    }

    [Fact]
    public void Test_ExecuteAsync_When_SetInterval_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";
        var countExecuter = 0;

        var source = new CancellationTokenSource();
        var token = source.Token;

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";
        void AfterExecuter(string data) => countExecuter++;
        void AfterWork() => source.Cancel();

        // Act
        var job = Job.Create(
            requesterUnique: Requester,
            executer: Executer,
            afterExecuter: AfterExecuter,
            afterWork: AfterWork
        );
        _jobService.ExecuteAsync(job, TimeSpan.FromSeconds(1), token);

        // Assert
        Assert.Equal("processing => data => Executer", result);
    }
}
