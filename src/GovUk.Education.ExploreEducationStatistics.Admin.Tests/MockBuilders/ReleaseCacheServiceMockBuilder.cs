#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleaseCacheServiceMockBuilder
{
    private readonly Mock<IReleaseCacheService> _mock = new(MockBehavior.Strict);

    public IReleaseCacheService Build()
    {
        _mock.Setup(m => m.RemoveRelease(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        return _mock.Object;
    }
}
