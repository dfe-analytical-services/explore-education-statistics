#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class PublicationCacheServiceMockBuilder
{
    private readonly Mock<IPublicationCacheService> _mock = new(MockBehavior.Strict);

    public PublicationCacheServiceMockBuilder()
    {
        _mock.Setup(m => m.RemovePublication(It.IsAny<string>())).Returns(Task.CompletedTask);
    }

    public IPublicationCacheService Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationCacheService> mock)
    {
        public void CacheInvalidatedForPublicationAndReleases(string publicationSlug) =>
            mock.Verify(m => m.RemovePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Once);

        public void CacheNotInvalidatedForPublicationAndReleases(string publicationSlug) =>
            mock.Verify(m => m.RemovePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Never);
    }
}
