using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class MethodologyServiceBuilder
{
    private readonly Mock<IMethodologyService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public IMethodologyService Build() => _mock.Object;

    public MethodologyServiceBuilder WhereGetLatestVersionByReleaseReturns(
        ReleaseVersion releaseVersion,
        params MethodologyVersion[] methodologyVersions)
    {
        _mock
            .Setup(m => m.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(methodologyVersions.ToList());
        
        _mock
            .Setup(m => m.Publish(It.IsAny<MethodologyVersion>()))
            .Returns(Task.CompletedTask);
        
        return this;
    }
    
    public MethodologyServiceBuilder WhereGetLatestVersionByReleaseReturnsNoMethodologies(ReleaseVersion releaseVersion)
    {
        _mock
            .Setup(m => m.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync([]);
        
        return this;
    }

    public MethodologyServiceBuilder WhereIsBeingPublishedAlongsideRelease(MethodologyVersion methodologyVersion, ReleaseVersion releaseVersion)
    {
        _mock
            .Setup(m => m.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
            .ReturnsAsync(true);
        
        return this;
    }
    public MethodologyServiceBuilder WhereIsNotBeingPublishedAlongsideRelease(MethodologyVersion methodologyVersion, ReleaseVersion releaseVersion)
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
