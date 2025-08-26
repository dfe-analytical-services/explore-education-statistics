#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleaseCacheServiceMockBuilder
{
    private readonly Mock<IReleaseCacheService> _mock = new(MockBehavior.Strict);
    public IReleaseCacheService Build()
    {
        _mock
            .Setup(m => m.UpdateRelease(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Guid releaseVersionId, string _, string _) => new ReleaseCacheViewModel(releaseVersionId));
        
        _mock
            .Setup(m => m.RemoveRelease(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => Unit.Instance);
        
        return _mock.Object;
    }
}
