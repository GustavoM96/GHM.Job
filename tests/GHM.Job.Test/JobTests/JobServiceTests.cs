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
    public async Task Test_ExecuteAsync_When_SetInterval_ShouldRun_JobDoWork()
    {
        // Arrange
        var result = "processing";

        var source = new CancellationTokenSource();
        var token = source.Token;

        string Requester() => " => data";
        string Executer(string data) => result += data + " => Executer";
        void AfterWork() => source.Cancel();

        // Act
        var job = Job.Create(requesterUnique: Requester, executer: Executer, afterWork: AfterWork);
        await _jobService.ExecuteAsync(job, TimeSpan.FromSeconds(1), token);

        // Assert
        Assert.Equal("processing => data => Executer", result);
    }
}
