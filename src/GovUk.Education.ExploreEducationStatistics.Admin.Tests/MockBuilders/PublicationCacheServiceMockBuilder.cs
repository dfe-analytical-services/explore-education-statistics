#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class PublicationCacheServiceMockBuilder
{
    private readonly Mock<IPublicationCacheService> _mock = new(MockBehavior.Strict);

    public PublicationCacheServiceMockBuilder()
    {
        _mock.Setup(m => m.UpdatePublicationTree()).ReturnsAsync([]);
    }

    public IPublicationCacheService Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationCacheService> mock)
    {
        public void CacheInvalidatedForPublicationTree() => mock.Verify(m => m.UpdatePublicationTree(), Times.Once);

        public void CacheNotInvalidatedForPublicationTree() => mock.Verify(m => m.UpdatePublicationTree(), Times.Never);
    }
}
