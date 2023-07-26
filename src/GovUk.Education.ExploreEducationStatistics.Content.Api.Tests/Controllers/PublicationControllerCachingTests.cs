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
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class PublicationControllerCachingTests : CacheServiceTestFixture
{
    private readonly PublicationsListRequest _query = new(
        ReleaseType.ExperimentalStatistics,
        ThemeId: Guid.Empty,
        Search: "",
        IPublicationService.PublicationsSortBy.Published,
        SortOrder.Asc,
        Page: 1,
        PageSize: 10
    );
    
    private readonly PaginatedListViewModel<PublicationSearchResultViewModel> _publications = new(
        ListOf(new PublicationSearchResultViewModel
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow,
            Rank = 4,
            Slug = "slug",
            Summary = "summary",
            Theme = "theme",
            Title = "title",
            Type = ReleaseType.ExperimentalStatistics
        }), 5, 1, 10);
        
    [Fact]
    public async Task ListPublications_NoCachedEntryExists()
    {
        var publicationService = new Mock<IPublicationService>(Strict);
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetPublicationListCacheKey(_query),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns(null);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));
        
        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetPublicationListCacheKey(_query),
                _publications,
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null));
        
        publicationService
            .Setup(s => s.ListPublications(
                _query.ReleaseType,
                _query.ThemeId,
                _query.Search,
                _query.Sort,
                _query.Order,
                _query.Page,
                _query.PageSize))
            .ReturnsAsync(_publications);
        
        var controller = BuildController(publicationService.Object);

        var result = await controller.ListPublications(_query);
        
        VerifyAllMocks(MemoryCacheService, publicationService);

        result.AssertOkResult(_publications);
    }
    
    [Fact]
    public async Task ListPublications_CachedEntryExists()
    {
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetPublicationListCacheKey(_query),
                typeof(PaginatedListViewModel<PublicationSearchResultViewModel>)))
            .Returns(_publications);
        
        var controller = BuildController();

        var result = await controller.ListPublications(_query);
        
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
