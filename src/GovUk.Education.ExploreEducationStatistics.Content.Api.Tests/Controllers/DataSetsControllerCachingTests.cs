#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using NCrontab;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class DataSetsControllerCachingTests
{
    public class ListDataSetsTests : CacheServiceTestFixture
    {
        private readonly DataSetsListRequest _query = new(
            ThemeId: Guid.NewGuid(),
            PublicationId: Guid.NewGuid(),
            ReleaseId: Guid.NewGuid(),
            LatestOnly: true,
            SearchTerm: "term",
            DataSetsListRequestSortBy.Published,
            SortDirection.Asc,
            Page: 1,
            PageSize: 10
        );

        private readonly PaginatedListViewModel<DataSetListViewModel> _dataSets = new(
            new List<DataSetListViewModel>
            {
                new()
                {
                    FileId = Guid.NewGuid(),
                    Filename = "Filename.csv",
                    FileSize = "1 Mb",
                    Title = "Title of data set",
                    Content = "Summary of data set",
                    Theme = new IdTitleViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Title of theme"
                    },
                    Publication = new IdTitleViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Title of publication"
                    },
                    Release = new IdTitleViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Academic year 2001/02"
                    },
                    LatestData = true,
                    Published = DateTime.UtcNow
                }
            }, 1, 1, 10);

        [Fact]
        public async Task NoCachedEntryExists_CreatesCache()
        {
            var dataSetService = new Mock<IDataSetService>(Strict);

            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListDataSetsCacheKey(_query),
                    typeof(PaginatedListViewModel<DataSetListViewModel>)))
                .Returns((object?) null);

            var expectedCacheConfiguration = new MemoryCacheConfiguration(
                10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

            MemoryCacheService
                .Setup(s => s.SetItem<object>(
                    new ListDataSetsCacheKey(_query),
                    _dataSets,
                    ItIs.DeepEqualTo(expectedCacheConfiguration),
                    null));

            dataSetService
                .Setup(s => s.ListDataSets(
                    _query.ThemeId,
                    _query.PublicationId,
                    _query.ReleaseId,
                    _query.LatestOnly,
                    _query.SearchTerm,
                    _query.Sort,
                    _query.SortDirection,
                    _query.Page,
                    _query.PageSize,
                    default))
                .ReturnsAsync(_dataSets);

            var controller = BuildController(dataSetService.Object);

            var result = await controller.ListDataSets(_query);

            VerifyAllMocks(MemoryCacheService, dataSetService);

            result.AssertOkResult(_dataSets);
        }

        [Fact]
        public async Task CachedEntryExists_ReturnsCache()
        {
            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListDataSetsCacheKey(_query),
                    typeof(PaginatedListViewModel<DataSetListViewModel>)))
                .Returns(_dataSets);

            var controller = BuildController();

            var result = await controller.ListDataSets(_query);

            VerifyAllMocks(MemoryCacheService);

            result.AssertOkResult(_dataSets);
        }

        [Fact]
        public void PaginatedDataSetListViewModel_SerializeAndDeserialize_Success()
        {
            var converted = JsonConvert.DeserializeObject<PaginatedListViewModel<DataSetListViewModel>>(
                JsonConvert.SerializeObject(_dataSets));

            converted.AssertDeepEqualTo(_dataSets);
        }
    }

    private static DataSetsController BuildController(
        IDataSetService? dataSetService = null
    )
    {
        return new DataSetsController(
            dataSetService ?? Mock.Of<IDataSetService>(Strict)
        );
    }
}
