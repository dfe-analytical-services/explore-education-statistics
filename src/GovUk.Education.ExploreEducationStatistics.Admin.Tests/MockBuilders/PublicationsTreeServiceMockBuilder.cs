#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class PublicationsTreeServiceMockBuilder
{
    private readonly Mock<IPublicationsTreeService> _mock = new(MockBehavior.Strict);

    public PublicationsTreeServiceMockBuilder()
    {
        _mock.Setup(m => m.UpdateCachedPublicationsTree(It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    public IPublicationsTreeService Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationsTreeService> mock)
    {
        public void CacheInvalidatedForPublicationsTree() =>
            mock.Verify(m => m.UpdateCachedPublicationsTree(It.IsAny<CancellationToken>()), Times.Once);

        public void CacheNotInvalidatedForPublicationsTree() =>
            mock.Verify(m => m.UpdateCachedPublicationsTree(It.IsAny<CancellationToken>()), Times.Never);
    }
}
