using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsSitemapControllerTests
{
    private readonly PublicationsSitemapServiceMockBuilder _publicationsSitemapService = new();

    public class GetSitemapItemsTests : PublicationsSitemapControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsSitemap_ReturnsOk()
        {
            // Arrange
            PublicationSitemapPublicationDto[] sitemapItems =
            [
                new()
                {
                    Slug = "test-publication",
                    LastModified = DateTime.Parse("2024-02-05T09:36:45.00Z"),
                    Releases =
                    [
                        new PublicationSitemapReleaseDto
                        {
                            Slug = "test-release",
                            LastModified = DateTime.Parse("2024-01-03T10:14:23.00Z"),
                        },
                    ],
                },
            ];
            _publicationsSitemapService.WhereHasSitemapItems(sitemapItems);

            var sut = BuildController();

            // Act
            var result = await sut.GetSitemapItems();

            // Assert
            _publicationsSitemapService.Assert.GetSitemapItemsWasCalled();
            Assert.Equal(sitemapItems, result);
        }
    }

    private PublicationsSitemapController BuildController() => new(_publicationsSitemapService.Build());
}
