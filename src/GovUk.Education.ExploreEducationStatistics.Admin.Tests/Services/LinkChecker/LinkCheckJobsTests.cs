using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.LinkChecker;

public class LinkCheckJobTests
{
    [Fact]
    public void Cancel_ShouldSetStatusToCanceled_WhenNotCompleted()
    {
        // Arrange
        var job = new LinkCheckerJob { Id = Guid.NewGuid() };
        job.Status = LinkCheckerJobStatus.Running;

        // Act
        job.Cancel();

        // Assert
        Assert.Equal(LinkCheckerJobStatus.Canceled, job.Status);
        Assert.Equal("Canceled by user.", job.ErrorMessage);
        Assert.True(job.CancellationRequested);
    }

    [Fact]
    public void Cancel_ShouldNotChangeStatus_WhenAlreadyCompleted()
    {
        // Arrange
        var job = new LinkCheckerJob { Id = Guid.NewGuid() };
        job.Status = LinkCheckerJobStatus.Completed;

        // Act
        job.Cancel();

        // Assert
        Assert.Equal(LinkCheckerJobStatus.Completed, job.Status);
    }

    [Fact]
    public void CancellationRequested_ShouldReturnTrue_WhenCanceled()
    {
        // Arrange
        var job = new LinkCheckerJob { Id = Guid.NewGuid() };

        // Act
        job.Cancel();

        // Assert
        Assert.True(job.CancellationRequested);
    }
}
