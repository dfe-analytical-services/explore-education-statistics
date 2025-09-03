using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsSearchControllerTests
{
    private readonly PublicationsSearchServiceMockBuilder _publicationsSearchService = new();

    private readonly PaginatedListViewModel<PublicationSearchResultViewModel> _publications = new(
        [
            new PublicationSearchResultViewModel
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow,
                Rank = 1,
                Slug = "slug",
                LatestReleaseSlug = "latest-release-slug",
                Summary = "summary",
                Theme = "theme",
                Title = "title",
                Type = ReleaseType.OfficialStatistics
            }
        ],
        totalResults: 1,
        page: 1,
        pageSize: 10);

    public class GetPublicationsByGetRequestTests : PublicationsSearchControllerTests
    {
        [Fact]
        public async Task GetPublications_WhenServiceReturnsPublications_ReturnsSearchResults()
        {
            // Arrange
            var request = new PublicationsListGetRequest(
                ReleaseType.OfficialStatistics,
                ThemeId: Guid.NewGuid(),
                Search: "",
                PublicationsSortBy.Published,
                SortDirection.Asc,
                Page: 1,
                PageSize: 10);
            _publicationsSearchService.WhereHasPublications(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(request);

            // Assert
            _publicationsSearchService.Assert.GetSearchItemsWasCalledForRequest(request);
            Assert.Equal(_publications, result);
        }
    }

    public class GetPublicationsByPostRequestTests : PublicationsSearchControllerTests
    {
        [Fact]
        public async Task GetPublications_WhenServiceReturnsPublications_ReturnsSearchResults()
        {
            // Arrange
            var request = new PublicationsListPostRequest(
                ReleaseType.OfficialStatistics,
                ThemeId: Guid.NewGuid(),
                Search: "",
                PublicationsSortBy.Published,
                SortDirection.Asc,
                Page: 1,
                PageSize: 10,
                PublicationIds: [Guid.NewGuid()]);
            _publicationsSearchService.WhereHasPublications(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(request);

            // Assert
            _publicationsSearchService.Assert.GetSearchItemsWasCalledForRequest(request);
            Assert.Equal(_publications, result);
        }
    }

    private PublicationsSearchController BuildController() => new(_publicationsSearchService.Build());
}
