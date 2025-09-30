using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class MethodologyServiceMockBuilder
{
    private readonly Mock<IMethodologyService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public MethodologyServiceMockBuilder()
    {
        // By default, no methodologies for any release version
        _mock.Setup(m => m.GetLatestVersionByRelease(It.IsAny<ReleaseVersion>())).ReturnsAsync([]);
    }

    public IMethodologyService Build() => _mock.Object;

    public MethodologyServiceMockBuilder WhereGetLatestVersionByReleaseReturns(
        ReleaseVersion releaseVersion,
        params MethodologyVersion[] methodologyVersions
    )
    {
        _mock
            .Setup(m => m.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(methodologyVersions.ToList());

        _mock.Setup(m => m.Publish(It.IsAny<MethodologyVersion>())).Returns(Task.CompletedTask);

        return this;
    }

    public MethodologyServiceMockBuilder WhereGetLatestVersionByReleaseReturnsNoMethodologies(
        ReleaseVersion releaseVersion
    )
    {
        _mock.Setup(m => m.GetLatestVersionByRelease(releaseVersion)).ReturnsAsync([]);

        return this;
    }

    public MethodologyServiceMockBuilder WhereIsBeingPublishedAlongsideRelease(
        MethodologyVersion methodologyVersion,
        ReleaseVersion releaseVersion
    )
    {
        _mock
            .Setup(m => m.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
            .ReturnsAsync(true);

        return this;
    }

    public MethodologyServiceMockBuilder WhereIsNotBeingPublishedAlongsideRelease(
        MethodologyVersion methodologyVersion,
        ReleaseVersion releaseVersion
    )
    {
        _mock
            .Setup(m => m.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
            .ReturnsAsync(false);

        return this;
    }

    public class Asserter(Mock<IMethodologyService> mock)
    {
        public void NoMethodologiesPublished()
        {
            mock.Verify(m => m.Publish(It.IsAny<MethodologyVersion>()), Times.Never());
        }

        public void MethodologyPublished(MethodologyVersion methodology)
        {
            mock.Verify(m => m.Publish(methodology), Times.Once);
        }

        public void MethodologyNotPublished(MethodologyVersion methodology)
        {
            mock.Verify(m => m.Publish(methodology), Times.Never);
        }
    }
}
