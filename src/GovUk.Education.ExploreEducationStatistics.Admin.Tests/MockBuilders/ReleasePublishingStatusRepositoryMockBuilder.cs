using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleasePublishingStatusRepositoryMockBuilder
{
    private readonly Mock<IReleasePublishingStatusRepository> _mock = new(MockBehavior.Strict);
    public IReleasePublishingStatusRepository Build() => _mock.Object;

    public ReleasePublishingStatusRepositoryMockBuilder SetReleaseVersionStatus(
        Guid releaseVersionId,
        params ReleasePublishingStatus[] releaseStatus)
    {
        _mock
            .Setup(m => m.GetAllByOverallStage(
                It.Is<Guid>(g => g == releaseVersionId),
                It.IsAny<ReleasePublishingStatusOverallStage[]>()))
            .ReturnsAsync(() => releaseStatus);
        
        return this;
    }
    
    public ReleasePublishingStatusRepositoryMockBuilder SetNoReleaseVersionStatus(
        Guid releaseVersionId)
    {
        _mock
            .Setup(m => m.GetAllByOverallStage(
                It.Is<Guid>(g => g == releaseVersionId),
                It.IsAny<ReleasePublishingStatusOverallStage[]>()))
            .ReturnsAsync(Array.Empty<ReleasePublishingStatus>);
        
        return this;
    }
}
