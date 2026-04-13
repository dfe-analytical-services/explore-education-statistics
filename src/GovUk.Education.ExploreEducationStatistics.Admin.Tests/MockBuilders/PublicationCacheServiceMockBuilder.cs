#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class PublicationCacheServiceMockBuilder
{
    private readonly Mock<IPublicationCacheService> _mock = new(MockBehavior.Strict);

    public PublicationCacheServiceMockBuilder()
    {
        _mock
            .Setup(m => m.UpdatePublication(It.IsAny<string>()))
            .ReturnsAsync((string slug) => new PublicationCacheViewModel { Slug = slug });

        _mock.Setup(m => m.RemovePublication(It.IsAny<string>())).ReturnsAsync(Unit.Instance);
    }

    public IPublicationCacheService Build() => _mock.Object;

    public PublicationCacheServiceMockBuilder WhereInvalidatingPublicationThrows(string? publicationSlug = null)
    {
        _mock
            .Setup(m =>
                m.UpdatePublication(It.Is<string>(actual => publicationSlug == null || actual == publicationSlug))
            )
            .Throws((string slug) => new Exception("This is a mock exception: UpdatePublication: " + slug));
        return this;
    }

    public PublicationCacheServiceMockBuilder WhereInvalidatingPublicationFails(string? publicationSlug = null)
    {
        _mock
            .Setup(m =>
                m.UpdatePublication(It.Is<string>(actual => publicationSlug == null || actual == publicationSlug))
            )
            .ReturnsAsync(() => new NotFoundResult());
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationCacheService> mock)
    {
        public void CacheInvalidatedForPublicationAndReleases(string publicationSlug) =>
            mock.Verify(m => m.RemovePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Once);

        public void CacheNotInvalidatedForPublicationAndReleases(string publicationSlug) =>
            mock.Verify(m => m.RemovePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Never);

        public void CacheInvalidatedForPublicationEntry(string publicationSlug) =>
            mock.Verify(m => m.UpdatePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Once);

        public void CacheNotInvalidatedForPublicationEntry(string publicationSlug) =>
            mock.Verify(m => m.UpdatePublication(It.Is<string>(actual => actual == publicationSlug)), Times.Never);
    }
}
