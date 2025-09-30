using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class ReleaseServiceMockBuilder
{
    private readonly Mock<IReleaseService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public IReleaseService Build() => _mock.Object;

    public ReleaseServiceMockBuilder()
    {
        _mock
            .Setup(m => m.CompletePublishing(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);
    }

    public ReleaseServiceMockBuilder WhereGetReturns(ReleaseVersion releaseVersion)
    {
        _mock.Setup(m => m.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        return this;
    }

    public ReleaseServiceMockBuilder WherePublicationLatestPublishedReleaseVersionIs(
        Guid publicationId,
        ReleaseVersion latestPublishedReleaseVersion
    )
    {
        _mock
            .Setup(m =>
                m.GetLatestPublishedReleaseVersion(publicationId, It.IsAny<IReadOnlyList<Guid>?>())
            )
            .ReturnsAsync(() => latestPublishedReleaseVersion);

        return this;
    }

    public ReleaseVersion Get(Guid releaseVersionId) => _mock.Object.Get(releaseVersionId).Result;

    public class Asserter(Mock<IReleaseService> mock)
    {
        public void CompletePublishingWasCalled(Guid releaseVersionId)
        {
            mock.Verify(
                m => m.CompletePublishing(releaseVersionId, It.IsAny<DateTime>()),
                Times.Once
            );
        }
    }
}
