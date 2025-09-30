#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleaseVersionServiceMockBuilder
{
    private readonly Mock<IReleaseVersionService> _mock = new(MockBehavior.Strict);

    public IReleaseVersionService Build() => _mock.Object;

    public ReleaseVersionServiceMockBuilder WhereGetReleaseVersionReturns(
        Guid? releaseVersionId = null,
        ReleaseVersionViewModel? releaseVersionViewModel = null
    )
    {
        _mock
            .Setup(rvs =>
                rvs.GetRelease(
                    It.Is<Guid>(id => releaseVersionId == null || id == releaseVersionId)
                )
            )
            .ReturnsAsync(releaseVersionViewModel ?? new ReleaseVersionViewModel());
        return this;
    }
}
