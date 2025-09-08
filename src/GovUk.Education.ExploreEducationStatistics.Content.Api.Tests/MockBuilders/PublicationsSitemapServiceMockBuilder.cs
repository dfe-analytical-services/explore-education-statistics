using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationsSitemapServiceMockBuilder
{
    private readonly Mock<IPublicationsSitemapService> _mock = new(MockBehavior.Strict);

    private PublicationSitemapPublicationDto[]? _sitemapItems;

    private static readonly Expression<
        Func<IPublicationsSitemapService, Task<PublicationSitemapPublicationDto[]>>
    > GetSitemapItems = m => m.GetSitemapItems(It.IsAny<CancellationToken>());

    public PublicationsSitemapServiceMockBuilder()
    {
        _mock.Setup(GetSitemapItems).ReturnsAsync(() => _sitemapItems ?? []);
    }

    public IPublicationsSitemapService Build() => _mock.Object;

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
            mock.Verify(GetSitemapItems, Times.Once);
        }
    }
}
