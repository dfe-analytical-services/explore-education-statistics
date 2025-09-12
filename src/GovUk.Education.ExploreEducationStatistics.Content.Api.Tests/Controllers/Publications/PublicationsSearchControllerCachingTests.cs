using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsSearchControllerCachingTests : CacheServiceTestFixture
{
    private readonly PublicationsSearchServiceMockBuilder _publicationsSearchService = new();

    private readonly PublicationsListGetRequest _getQuery = new(
        ReleaseType.OfficialStatistics,
        ThemeId: Guid.NewGuid(),
        Search: "",
        PublicationsSortBy.Published,
        SortDirection.Asc,
        Page: 1,
        PageSize: 10
    );

    private readonly PublicationsListPostRequest _postQuery = new(
        ReleaseType.OfficialStatistics,
        ThemeId: Guid.NewGuid(),
        Search: "",
        PublicationsSortBy.Published,
        SortDirection.Asc,
        Page: 1,
        PageSize: 10,
        PublicationIds: [Guid.NewGuid()]
    );

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

    public class GetPublicationsByGetRequestTests : PublicationsSearchControllerCachingTests
    {
        [Fact]
        public async Task WhenNoCacheEntryExists_InvokesServiceAndCreateCache()
        {
            // Arrange
            MemoryCacheService
                .SetupNotFoundForAnyKey<ListPublicationsGetCacheKey,
                    PaginatedListViewModel<PublicationSearchResultViewModel>>();
            _publicationsSearchService.WhereHasPublications(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(_getQuery);

            // Assert
            _publicationsSearchService.Assert.GetPublicationsWasCalledForRequest(_getQuery);
            Assert.Equal(_publications, result);
            Mock.VerifyAll(MemoryCacheService);
        }

        [Fact]
        public async Task WhenCachedEntryExists_ReturnsCachedSearchResults()
        {
            // Arrange
            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListPublicationsGetCacheKey(_getQuery),
                    typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
                .Returns(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(_getQuery);

            // Assert
            _publicationsSearchService.Assert.GetPublicationsWasNotCalled();
            Assert.Equal(_publications, result);
            Mock.VerifyAll(MemoryCacheService);
        }

        [Fact]
        public void PublicationSearchResultViewModel_SerializeAndDeserialize_MaintainsEquality()
        {
            var converted = JsonConvert.DeserializeObject<PaginatedListViewModel<PublicationSearchResultViewModel>>(
                JsonConvert.SerializeObject(_publications));

            converted.AssertDeepEqualTo(_publications);
        }
    }

    public class GetPublicationsByPostRequestTests : PublicationsSearchControllerCachingTests
    {
        [Fact]
        public async Task WhenNoCacheEntryExists_InvokesServiceAndCreatesCache()
        {
            // Arrange
            MemoryCacheService
                .SetupNotFoundForAnyKey<ListPublicationsPostCacheKey,
                    PaginatedListViewModel<PublicationSearchResultViewModel>>();
            _publicationsSearchService.WhereHasPublications(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(_postQuery);

            // Assert
            _publicationsSearchService.Assert.GetPublicationsWasCalledForRequest(_postQuery);
            Assert.Equal(_publications, result);
            Mock.VerifyAll(MemoryCacheService);
        }

        [Fact]
        public async Task WhenCachedEntryExists_ReturnsCachedSearchResults()
        {
            // Arrange
            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListPublicationsPostCacheKey(_postQuery),
                    typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
                .Returns(_publications);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublications(_postQuery);

            // Assert
            _publicationsSearchService.Assert.GetPublicationsWasNotCalled();
            Assert.Equal(_publications, result);
            Mock.VerifyAll(MemoryCacheService);
        }
    }

    private PublicationsSearchController BuildController() => new(_publicationsSearchService.Build());
}
