#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationsSitemapServiceMockBuilder
{
    private readonly Mock<IPublicationsSitemapService> _mock = new(MockBehavior.Strict);

    private PublicationSitemapPublicationDto[]? _sitemapItems;

    public IPublicationsSitemapService Build()
    {
        _mock.Setup(m => m.GetSitemapItems(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sitemapItems ?? []);

        return _mock.Object;
    }

    public PublicationsSitemapServiceMockBuilder WhereHasSitemapItems(PublicationSitemapPublicationDto[] sitemapItems)
    {
        _sitemapItems = sitemapItems;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationsSitemapService> mock)
    {
        public void GetSitemapItemsWasCalled()
        {
            mock.Verify(m => m.GetSitemapItems(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
