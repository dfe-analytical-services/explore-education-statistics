using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public delegate void TryGetJobCallback(Guid jobId, out LinkCheckerJob job);

public class BauLinksCheckerControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<ILinkCheckerQueue> _mockQueue = new();

    [Fact]
    public void StartLinkCheck_ShouldReturnAccepted_WithJobId()
    {
        // Arrange
        var expectedJobId = Guid.NewGuid();
        _mockQueue.Setup(q => q.EnqueueJob()).Returns(expectedJobId);

        var client = BuildController(_mockQueue.Object);

        // Act
        var response = client.StartLinkCheck();

        // Assert
        var result = response.AssertAcceptedObjectResult<StartLinkResponse>();
        Assert.Equal(expectedJobId.ToString(), result.Id.ToString());
    }

    [Fact]
    public void GetLinkCheckStatus_ShouldReturnOk_WithJobDetails()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new LinkCheckerJob
        {
            Id = jobId,
            Status = LinkCheckerJobStatus.Completed,
            Results = new List<LinksCsvItem>()
            {
                new(
                    "Test Publication",
                    "Test Heading",
                    "Test Slug",
                    "Test Slug2",
                    "test url",
                    "Test link test",
                    "localhost",
                    200
                ),
                new(
                    "Test Publication 2",
                    "Test Heading 2",
                    "Test Slug 2",
                    "Test Slug2 2",
                    "test url 2",
                    "Test link test 2",
                    "localhost",
                    404
                ),
            },
        };

        _mockQueue
            .Setup(q => q.TryGetJob(jobId, out It.Ref<LinkCheckerJob>.IsAny))
            .Callback(new TryGetJobCallback((Guid id, out LinkCheckerJob outJob) => outJob = job))
            .Returns(true);
        var client = BuildController(_mockQueue.Object);

        // Act
        var response = client.GetLinkCheckStatus(jobId);

        // Assert
        var result = response.AssertOkObjectResult<LinkCheckerJobDetails>();
        Assert.Equal(job.Id, result.Id);
        Assert.Equal(Enum.GetName(typeof(LinkCheckerJobStatus), job.Status), result.Status);
        Assert.Equal(1, job.BrokenLinks);
        Assert.Equal(2, job.TotalLinks);
    }

    [Fact]
    public void GetLinkCheckStatus_ShouldReturnNotFound_ForInvalidJobId()
    {
        // Arrange
        var client = BuildController(_mockQueue.Object);
        var jobId = Guid.NewGuid();

        // Act
        var response = client.GetLinkCheckStatus(jobId);

        // Assert
        response.AssertNotFoundResult();
    }

    [Fact]
    public void CancelLinkCheck_ShouldReturnAccepted_ForRunningJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new LinkCheckerJob { Id = jobId, Status = LinkCheckerJobStatus.Running };

        _mockQueue.Setup(q => q.TryGetJob(jobId, out job)).Returns(true);
        _mockQueue.Setup(q => q.TryCancelJob(jobId)).Returns(true);
        var client = BuildController(_mockQueue.Object);

        // Act
        var response = client.CancelLinkCheck(jobId);

        // Assert
        response.AssertAccepted();
    }

    [Fact]
    public void CancelLinkCheck_ShouldReturnBadRequest_ForCompletedJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new LinkCheckerJob { Id = jobId, Status = LinkCheckerJobStatus.Completed };

        _mockQueue.Setup(q => q.TryGetJob(jobId, out job)).Returns(true);
        var client = BuildController(_mockQueue.Object);

        // Act
        var response = client.CancelLinkCheck(jobId);

        // Assert
        Assert.IsAssignableFrom<BadRequestObjectResult>(response);
    }

    private BauLinksCheckerController BuildController(ILinkCheckerQueue queue = null)
    {
        var controller = new BauLinksCheckerController(queue ?? Mock.Of<ILinkCheckerQueue>(MockBehavior.Strict));
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>()))
            .Returns((string)null);
        controller.Url = urlHelper.Object;
        return controller;
    }
}
