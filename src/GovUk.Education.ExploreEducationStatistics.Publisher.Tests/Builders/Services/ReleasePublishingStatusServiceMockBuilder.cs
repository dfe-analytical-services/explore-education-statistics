using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class ReleasePublishingStatusServiceMockBuilder
{
    private readonly Mock<IReleasePublishingStatusService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public IReleasePublishingStatusService Build() => _mock.Object;

    public ReleasePublishingStatusServiceMockBuilder()
    {
        _mock
            .Setup(
                m => m.UpdatePublishingStage(
                    It.IsAny<ReleasePublishingKey>(),
                    It.IsAny<ReleasePublishingStatusPublishingStage>(),
                    It.IsAny<ReleasePublishingStatusLogMessage?>()))
            .Returns(Task.CompletedTask);

    }
    
    public ReleasePublishingStatusServiceMockBuilder WhereGetReturns(
        ReleasePublishingKey releasePublishingKey,
        ReleasePublishingStatus releasePublishingStatus)
    {
        _mock
            .Setup(m => m.Get(releasePublishingKey))
            .ReturnsAsync(releasePublishingStatus);
        return this;
    }
    
    public class Asserter(Mock<IReleasePublishingStatusService> mock)
    {
        public void UpdatePublishingStageWasNotCalled()
        {
            mock.Verify(
                m => m.UpdatePublishingStage(
                    It.IsAny<ReleasePublishingKey>(),
                    It.IsAny<ReleasePublishingStatusPublishingStage>(),
                    It.IsAny<ReleasePublishingStatusLogMessage?>()
                ),
                Times.Never);
        }

        public void UpdatePublishingStageWasNotCalled(ReleasePublishingKey key)
        {
            mock.Verify(
                m => m.UpdatePublishingStage(
                    key,
                    It.IsAny<ReleasePublishingStatusPublishingStage>(),
                    It.IsAny<ReleasePublishingStatusLogMessage?>()
                ),
                Times.Never);
        }
        
        public void UpdatePublishingStageWasCalled(ReleasePublishingKey key, ReleasePublishingStatusPublishingStage publishingStage)
        {
            mock.Verify(
                m => m.UpdatePublishingStage(
                    key,
                    publishingStage,
                    It.IsAny<ReleasePublishingStatusLogMessage?>()
                ),
                Times.Once);
        }
    }
}
