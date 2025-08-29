#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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

public abstract class DataSetFilesControllerCachingTests : CacheServiceTestFixture
{
    public class ListDataSetsTests : DataSetFilesControllerCachingTests
    {
        private readonly DataSetFileListRequest _query = new(
            ThemeId: Guid.NewGuid(),
            PublicationId: Guid.NewGuid(),
            ReleaseId: Guid.NewGuid(),
            GeographicLevel: GeographicLevel.Country.GetEnumValue(),
            LatestOnly: true,
            DataSetType: DataSetType.Api,
            SearchTerm: "term",
            DataSetsListRequestSortBy.Published,
            SortDirection.Asc,
            Page: 1,
            PageSize: 10
        );

        private readonly PaginatedListViewModel<DataSetFileSummaryViewModel> _dataSetFiles = new(
            [
                new DataSetFileSummaryViewModel
                {
                    Id = Guid.NewGuid(),
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
                    Publication = new IdTitleSlugViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Title of publication",
                        Slug = "publication-slug"
                    },
                    Release = new IdTitleSlugViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Academic year 2001/02",
                        Slug = "release-slug"
                    },
                    Api = new DataSetFileApiViewModel
                    {
                        Id = Guid.NewGuid(),
                        Version = "1.0.0"
                    },
                    Meta = new DataSetFileMetaViewModel
                    {
                        NumDataFileRows = 9393,
                        GeographicLevels = [GeographicLevel.Country.GetEnumLabel()],
                        TimePeriodRange = new DataSetFileTimePeriodRangeViewModel
                        {
                            From = "2000",
                            To = "2001",
                        },
                        Filters = ["Filter 1"],
                        Indicators = ["Indicator 1"],
                    },
                    LatestData = true,
                    IsSuperseded = false,
                    Published = DateTime.UtcNow,
                }
            ],
            totalResults: 1,
            page: 1,
            pageSize: 10);

        [Fact]
        public async Task NoCachedEntryExists_CreatesCache()
        {
            var dataSetFileService = new Mock<IDataSetFileService>(Strict);

            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListDataSetFilesCacheKey(_query),
                    typeof(PaginatedListViewModel<DataSetFileSummaryViewModel>)))
                .Returns((object?)null);

            var expectedCacheConfiguration = new MemoryCacheConfiguration(
                10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

            MemoryCacheService
                .Setup(s => s.SetItem<object>(
                    new ListDataSetFilesCacheKey(_query),
                    _dataSetFiles,
                    ItIs.DeepEqualTo(expectedCacheConfiguration),
                    null));

            dataSetFileService
                .Setup(s => s.ListDataSetFiles(
                    _query.ThemeId,
                    _query.PublicationId,
                    _query.ReleaseId,
                    _query.GeographicLevelEnum,
                    _query.LatestOnly,
                    _query.DataSetType,
                    _query.SearchTerm,
                    _query.Sort,
                    _query.SortDirection,
                    _query.Page,
                    _query.PageSize,
                    default))
                .ReturnsAsync(_dataSetFiles);

            var controller = BuildController(dataSetFileService.Object);

            var result = await controller.ListDataSetFiles(_query);

            VerifyAllMocks(MemoryCacheService, dataSetFileService);

            result.AssertOkResult(_dataSetFiles);
        }

        [Fact]
        public async Task CachedEntryExists_ReturnsCache()
        {
            MemoryCacheService
                .Setup(s => s.GetItem(
                    new ListDataSetFilesCacheKey(_query),
                    typeof(PaginatedListViewModel<DataSetFileSummaryViewModel>)))
                .Returns(_dataSetFiles);

            var controller = BuildController();

            var result = await controller.ListDataSetFiles(_query);

            VerifyAllMocks(MemoryCacheService);

            result.AssertOkResult(_dataSetFiles);
        }

        [Fact]
        public void PaginatedDataSetListViewModel_SerializeAndDeserialize_Success()
        {
            var converted = JsonConvert.DeserializeObject<PaginatedListViewModel<DataSetFileSummaryViewModel>>(
                JsonConvert.SerializeObject(_dataSetFiles));

            converted.AssertDeepEqualTo(_dataSetFiles);
        }
    }

    private static DataSetFilesController BuildController(IDataSetFileService? dataSetFileService = null)
    {
        return new DataSetFilesController(dataSetFileService ?? Mock.Of<IDataSetFileService>(Strict));
    }
}
