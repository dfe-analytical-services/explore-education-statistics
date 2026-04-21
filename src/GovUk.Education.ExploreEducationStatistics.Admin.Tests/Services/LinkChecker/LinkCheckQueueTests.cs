using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.LinkChecker;

public class LinkCheckQueueTests
{
    private readonly LinkCheckerQueue _queue = new();

    [Fact]
    public async Task EnqueueJob_ShouldReturnUniqueId()
    {
        // Act
        var id1 = _queue.EnqueueJob();
        var id2 = _queue.EnqueueJob();

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.True(_queue.TryGetJob(id1, out _));
        Assert.True(_queue.TryGetJob(id2, out _));
    }

    [Fact]
    public async Task TryGetJob_ShouldReturnFalse_ForNonExistentJob()
    {
        // Act
        var result = _queue.TryGetJob(Guid.NewGuid(), out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TryCancelJob_ShouldCancelPendingJob()
    {
        // Arrange
        var jobId = _queue.EnqueueJob();

        // Act
        var canceled = _queue.TryCancelJob(jobId);

        // Assert
        Assert.True(canceled);
        Assert.True(_queue.TryGetJob(jobId, out var job));
        Assert.Equal(LinkCheckerJobStatus.Canceled, job!.Status);
    }

    [Fact]
    public async Task TryCancelJob_ShouldNotCancelCompletedJob()
    {
        // Arrange
        var jobId = _queue.EnqueueJob();
        _queue.TryGetJob(jobId, out var job);
        job!.Status = LinkCheckerJobStatus.Completed;

        // Act
        var canceled = _queue.TryCancelJob(jobId);

        // Assert
        Assert.False(canceled);
        Assert.Equal(LinkCheckerJobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task DequeueAsync_ShouldReturnEnqueuedJob()
    {
        // Arrange
        var jobId = _queue.EnqueueJob();

        // Act
        var job = await _queue.DequeueAsync(CancellationToken.None);

        // Assert
        Assert.Equal(jobId, job.Id);
    }
}
