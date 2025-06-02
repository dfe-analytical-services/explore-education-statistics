#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class PublicationControllerCachingTests : CacheServiceTestFixture
{
    private readonly PublicationsListGetRequest _getQuery = new(
        ReleaseType.ExperimentalStatistics,
        ThemeId: Guid.Empty,
        Search: "",
        PublicationsSortBy.Published,
        SortDirection.Asc,
        Page: 1,
        PageSize: 10
    );

    private readonly PublicationsListPostRequest _postQuery = new(
        ReleaseType.ExperimentalStatistics,
        ThemeId: Guid.Empty,
        Search: "",
        PublicationsSortBy.Published,
        SortDirection.Asc,
        Page: 1,
        PageSize: 10,
        PublicationIds: ListOf(Guid.Empty)
    );

    private readonly PaginatedListViewModel<PublicationSearchResultViewModel> _publications = new(
        ListOf(new PublicationSearchResultViewModel
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow,
            Rank = 4,
            Slug = "slug",
            LatestReleaseSlug = "latest-release-slug",
            Summary = "summary",
            Theme = "theme",
            Title = "title",
            Type = ReleaseType.ExperimentalStatistics
        }), 5, 1, 10);

    [Fact]
    public async Task ListPublications_GetRequest_NoCachedEntryExists_CreatesCache()
    {
        var publicationService = new Mock<IPublicationService>(Strict);

        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListPublicationsGetCacheKey(_getQuery),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns((object?)null);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new ListPublicationsGetCacheKey(_getQuery),
                _publications,
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null));

        publicationService
            .Setup(s => s.ListPublications(
                _getQuery.ReleaseType,
                _getQuery.ThemeId,
                _getQuery.Search,
                _getQuery.Sort,
                _getQuery.SortDirection,
                _getQuery.Page,
                _getQuery.PageSize,
                null))
            .ReturnsAsync(_publications);

        var controller = BuildController(publicationService.Object);

        var result = await controller.ListPublications(_getQuery);

        VerifyAllMocks(MemoryCacheService, publicationService);

        result.AssertOkResult(_publications);
    }

    [Fact]
    public async Task ListPublications_GetRequest_CachedEntryExists_ReturnsCache()
    {
        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListPublicationsGetCacheKey(_getQuery),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns(_publications);

        var controller = BuildController();

        var result = await controller.ListPublications(_getQuery);

        VerifyAllMocks(MemoryCacheService);

        result.AssertOkResult(_publications);
    }

    [Fact]
    public async Task ListPublications_PostRequest_NoCachedEntryExists_CreatesCache()
    {
        var publicationService = new Mock<IPublicationService>(Strict);

        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListPublicationsPostCacheKey(_postQuery),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns((object?)null);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new ListPublicationsPostCacheKey(_postQuery),
                _publications,
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null));

        publicationService
            .Setup(s => s.ListPublications(
                _postQuery.ReleaseType,
                _postQuery.ThemeId,
                _postQuery.Search,
                _postQuery.Sort,
                _postQuery.SortDirection,
                _postQuery.Page,
                _postQuery.PageSize,
                _postQuery.PublicationIds))
            .ReturnsAsync(_publications);

        var controller = BuildController(publicationService.Object);

        var result = await controller.ListPublications(_postQuery);

        VerifyAllMocks(MemoryCacheService, publicationService);

        result.AssertOkResult(_publications);
    }

    [Fact]
    public async Task ListPublications_PostRequest_CachedEntryExists_ReturnsCache()
    {
        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListPublicationsPostCacheKey(_postQuery),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns(_publications);

        var controller = BuildController();

        var result = await controller.ListPublications(_postQuery);

        VerifyAllMocks(MemoryCacheService);

        result.AssertOkResult(_publications);
    }

    [Fact]
    public void PublicationSearchResults_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<PaginatedListViewModel<PublicationSearchResultViewModel>>(
            SerializeObject(_publications));

        converted.AssertDeepEqualTo(_publications);
    }

    private static PublicationController BuildController(
        IPublicationService? publicationService = null
    )
    {
        return new(
            Mock.Of<IPublicationCacheService>(Strict),
            publicationService ?? Mock.Of<IPublicationService>(Strict)
        );
    }
}
