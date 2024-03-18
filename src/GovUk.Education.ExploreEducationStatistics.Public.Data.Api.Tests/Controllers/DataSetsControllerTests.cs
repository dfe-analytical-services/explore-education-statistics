using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.WebUtilities;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerTests : IntegrationTestFixture
{
    private const string BaseUrl = "api/v1/data-sets";

    public DataSetsControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    public class GetDataSetTests : DataSetsControllerTests
    {
        public GetDataSetTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData(DataSetStatus.Published)]
        [InlineData(DataSetStatus.Deprecated)]
        public async Task DataSetIsAvailable_Returns200(DataSetStatus dataSetStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(dataSet);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var content = response.AssertOk<DataSetViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(dataSet.Id, content.Id);
            Assert.Equal(dataSet.Title, content.Title);
            Assert.Equal(dataSet.Summary, content.Summary);
            Assert.Equal(dataSet.Status, content.Status);
            Assert.Equal(dataSet.SupersedingDataSetId, content.SupersedingDataSetId);
            Assert.Equal(dataSetVersion!.Version, content.LatestVersion.Number);
            Assert.Equal(
                dataSetVersion.Published!.Value.ToUnixTimeSeconds(),
                content.LatestVersion.Published.ToUnixTimeSeconds()
            );
            Assert.Equal(dataSetVersion.TotalResults, content.LatestVersion.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                content.LatestVersion.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                content.LatestVersion.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, content.LatestVersion.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, content.LatestVersion.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, content.LatestVersion.Indicators);
        }

        [Theory]
        [InlineData(DataSetStatus.Draft)]
        [InlineData(DataSetStatus.Withdrawn)]
        public async Task DataSetNotAvailable_Returns403(DataSetStatus dataSetStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await GetDataSet(Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSet(Guid dataSetId)
        {
            var client = TestApp.CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListDataSetVersionsTests : DataSetsControllerTests
    {
        public ListDataSetVersionsTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData(1, 2, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(1, 2, 9)]
        [InlineData(1, 3, 2)]
        [InlineData(2, 2, 9)]
        public async Task MultipleAvailableVersionsForRequestedDataSet_Returns200_PaginatedCorrectly(
           int page,
           int pageSize,
           int numberOfAvailableDataSetVersions)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .GenerateList(numberOfAvailableDataSetVersions);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
            });

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            var expectedPageCount = dataSetVersions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Count();

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfAvailableDataSetVersions, content.Paging.TotalResults);
            Assert.Equal(expectedPageCount, content.Results.Count);
        }

        [Fact]
        public async Task MultipleAvailableVersionsForRequestedDataSet_Returns200_OrderedCorrectly()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, dsv => dsv.SetVersionNumber(1, 0))
                .ForIndex(1, dsv => dsv.SetVersionNumber(1, 1))
                .ForIndex(2, dsv => dsv.SetVersionNumber(3, 1))
                .ForIndex(3, dsv => dsv.SetVersionNumber(3, 0))
                .ForIndex(4, dsv => dsv.SetVersionNumber(2, 0))
                .ForIndex(5, dsv => dsv.SetVersionNumber(2, 1))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
            });

            var page1Response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 3);

            var page1Content = page1Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.Equal(3, page1Content.Results.Count);
            Assert.Equal("3.1", page1Content.Results[0].Number);
            Assert.Equal("3.0", page1Content.Results[1].Number);
            Assert.Equal("2.1", page1Content.Results[2].Number);

            var page2Response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 2,
                pageSize: 3);

            var page2Content = page2Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.Equal(3, page2Content.Results.Count);
            Assert.Equal("2.0", page2Content.Results[0].Number);
            Assert.Equal("1.1", page2Content.Results[1].Number);
            Assert.Equal("1.0", page2Content.Results[2].Number);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task DataSetVersionIsAvailable_Returns200_CorrectViewModel(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersionGenerator = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSetId(dataSet.Id);

            DataSetVersion dataSetVersion = dataSetVersionStatus == DataSetVersionStatus.Withdrawn
                ? dataSetVersionGenerator.WithWithdrawn(DateTimeOffset.UtcNow)
                : dataSetVersionGenerator;

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);

            var result = Assert.Single(content.Results);

            Assert.Equal(dataSetVersion.Version, result.Number);
            Assert.Equal(dataSetVersion.VersionType, result.Type);
            Assert.Equal(dataSetVersion.Status, result.Status);
            Assert.Equal(
                dataSetVersion.Published!.Value.ToUnixTimeSeconds(),
                result.Published.ToUnixTimeSeconds()
            );
            Assert.Equal(
                dataSetVersion.Withdrawn?.ToUnixTimeSeconds(),
                result.Withdrawn?.ToUnixTimeSeconds()
            );
            Assert.Equal(dataSetVersion.Notes, result.Notes);
            Assert.Equal(dataSetVersion.TotalResults, result.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                result.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                result.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, result.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.Indicators);
        }

        [Fact]
        public async Task AvailableVersionForOtherDataSet_Returns200_OnlyVersionForRequestedDataSet()
        {
            DataSet dataSet1 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet dataSet2 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSet1Version = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithVersionNumber(1, 1)
                .WithDataSetId(dataSet1.Id);

            DataSetVersion dataSet2Version = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithVersionNumber(2, 2)
                .WithDataSetId(dataSet2.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version));

            var response = await ListDataSetVersions(
                dataSetId: dataSet1.Id,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);
            var result = Assert.Single(content.Results);
            Assert.Equal(dataSet1Version.Version, result.Number);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task DataSetVersionUnavailable_Returns200_EmptyList(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Fact]
        public async Task NoDataSetVersions_Returns200_EmptyList()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Fact]
        public async Task PageTooBig_Returns200_EmptyList()
        {
            var page = 2;
            var pageSize = 2;
            var numberOfDataSetVersions = 2;

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .GenerateList(numberOfDataSetVersions);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.AddRange(dataSetVersions));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfDataSetVersions, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageTooSmall_Returns400(int page)
        {
            var response = await ListDataSetVersions(
                dataSetId: Guid.NewGuid(),
                page: page,
                pageSize: 1);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var response = await ListDataSetVersions(
                dataSetId: Guid.NewGuid(),
                page: 1,
                pageSize: pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize");
        }

        [Theory]
        [InlineData(DataSetStatus.Draft)]
        [InlineData(DataSetStatus.Withdrawn)]
        public async Task UnavailableDataSet_Returns503(DataSetStatus status)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(status);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await ListDataSetVersions(dataSetId: dataSet.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task InvalidDataSetId_Returns404()
        {
            var client = TestApp.CreateClient();

            var query = new Dictionary<string, string?>
            {
                { "page", "1" },
                { "pageSize", "1" },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/not_a_valid_guid/versions", query);

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> ListDataSetVersions(
            Guid dataSetId,
            int? page = null,
            int? pageSize = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/versions", query);

            var client = TestApp.CreateClient();

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetVersionTests : DataSetsControllerTests
    {
        public GetDataSetVersionTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task VersionIsAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(dataSet.Id, dataSetVersion.Version);

            var content = response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(dataSetVersion.Version, content.Number);
            Assert.Equal(dataSetVersion.VersionType, content.Type);
            Assert.Equal(dataSetVersion.Status, content.Status);
            Assert.Equal(
                dataSetVersion.Published!.Value.ToUnixTimeSeconds(),
                content.Published.ToUnixTimeSeconds()
            );
            Assert.Equal(
                dataSetVersion.Withdrawn?.ToUnixTimeSeconds(),
                content.Withdrawn?.ToUnixTimeSeconds()
            );
            Assert.Equal(dataSetVersion.Notes, content.Notes);
            Assert.Equal(dataSetVersion.TotalResults, content.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                content.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                content.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, content.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, content.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, content.Indicators);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(dataSet.Id, dataSetVersion.Version);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionExistsForOtherDataSet_Returns404()
        {
            DataSet dataSet1 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet dataSet2 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet1.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSetVersion(dataSet.Id, "1.0");

            response.AssertNotFound();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(Guid.NewGuid(), dataSetVersion.Version);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetVersion(Guid dataSetId, string dataSetVersion)
        {
            var client = TestApp.CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetMetaTests : DataSetsControllerTests
    {
        public GetDataSetMetaTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task VersionExists_ReturnsCorrectViewModel()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var filterMetas = DataFixture
                .DefaultFilterMeta()
                .WithOptions(() => DataFixture
                    .DefaultFilterOptionMeta()
                    .GenerateList(3))
                .GenerateList(3);

            var allLocationOptionMetaTypesGeneratorByLevel = new Dictionary<GeographicLevel, Func<LocationOptionMeta>>
            {
                { GeographicLevel.School, () => DataFixture.DefaultLocationSchoolOptionMeta() },
                { GeographicLevel.LocalAuthority, () => DataFixture.DefaultLocationLocalAuthorityOptionMeta() },
                { GeographicLevel.RscRegion, () => DataFixture.DefaultLocationRscRegionOptionMeta() },
                { GeographicLevel.Provider, () => DataFixture.DefaultLocationProviderOptionMeta() },
                { GeographicLevel.EnglishDevolvedArea, () => DataFixture.DefaultLocationCodedOptionMeta() },
            };

            var locationMetas = allLocationOptionMetaTypesGeneratorByLevel
                .Select(locationOptionMetaGenerator => DataFixture
                    .DefaultLocationMeta()
                    .WithOptions(() => new List<LocationOptionMeta>
                    {
                         locationOptionMetaGenerator.Value.Invoke(),
                         locationOptionMetaGenerator.Value.Invoke(),
                         locationOptionMetaGenerator.Value.Invoke()
                    })
                    .WithLevel(locationOptionMetaGenerator.Key))
                .Select(locationMeta => (LocationMeta)locationMeta)
                .ToList();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithFilterMetas(() => filterMetas)
                .WithLocationMetas(() => locationMetas)
                .WithIndicatorMetas(() =>
                    DataFixture
                    .DefaultIndicatorMeta()
                    .GenerateList(3)
                )
                .WithTimePeriodMetas(() =>
                    DataFixture
                    .DefaultTimePeriodMeta()
                    .GenerateList(3)
                )
                .WithMetaSummary();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetMeta(dataSet.Id, dataSetVersion.Version);

            var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

            Assert.NotNull(content);

            foreach (var filterMetaViewModel in content.Filters)
            {
                var filterMeta = Assert.Single(dataSetVersion.FilterMetas, fm => fm.PublicId == filterMetaViewModel.Id);
                Assert.Equal(filterMeta.Hint, filterMetaViewModel.Hint);
                Assert.Equal(filterMeta.Label, filterMetaViewModel.Label);

                var allFilterMetaLinks = filterMeta.Options
                    .SelectMany(o => o.MetaLinks)
                    .ToList();

                foreach (var filterOptionMetaViewModel in filterMetaViewModel.Options)
                {
                    var filterOptionMetaLink = Assert.Single(
                        allFilterMetaLinks,
                        link => link.PublicId == filterOptionMetaViewModel.Id);

                    var filterOptionMeta = Assert.Single(
                        filterMeta.Options,
                        o => o.Id == filterOptionMetaLink.OptionId);

                    Assert.Equal(filterOptionMeta.Label, filterOptionMetaViewModel.Label);
                    Assert.Equal(filterOptionMeta.IsAggregate, filterOptionMetaViewModel.IsAggregate);
                }
            }

            foreach (var locationMetaViewModel in content.Locations)
            {
                var locationMeta = Assert.Single(
                    dataSetVersion.LocationMetas,
                    fm => fm.Level == locationMetaViewModel.Level);

                var allLocationMetaLinks = locationMeta.Options
                    .SelectMany(o => o.MetaLinks)
                    .ToList();

                foreach (var locationOptionMetaViewModel in locationMetaViewModel.Options)
                {
                    var locationOptionMetaLink = Assert.Single(
                        allLocationMetaLinks,
                        link => link.PublicId == locationOptionMetaViewModel.Id);

                    var locationOptionMeta = Assert.Single(
                        locationMeta.Options,
                        o => o.Id == locationOptionMetaLink.OptionId);

                    switch (locationOptionMeta)
                    {
                        case LocationCodedOptionMeta codedMeta:
                            var codedViewModel =
                                Assert.IsType<LocationCodedOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(codedMeta.Label, codedViewModel.Label);
                            Assert.Equal(codedMeta.Code, codedViewModel.Code);
                            break;
                        case LocationLocalAuthorityOptionMeta localAuthorityMeta:
                            var localAuthorityViewModel =
                                Assert.IsType<LocationLocalAuthorityOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(localAuthorityMeta.Label, localAuthorityViewModel.Label);
                            Assert.Equal(localAuthorityMeta.Code, localAuthorityViewModel.Code);
                            Assert.Equal(localAuthorityMeta.OldCode, localAuthorityViewModel.OldCode);
                            break;
                        case LocationProviderOptionMeta providerMeta:
                            var providerViewModel =
                                Assert.IsType<LocationProviderOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(providerMeta.Label, providerViewModel.Label);
                            Assert.Equal(providerMeta.Ukprn, providerViewModel.Ukprn);
                            break;
                        case LocationRscRegionOptionMeta rscRegionMeta:
                            var rscRegionViewModel =
                                Assert.IsType<LocationRscRegionOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(rscRegionMeta.Label, rscRegionViewModel.Label);
                            break;
                        case LocationSchoolOptionMeta schoolMeta:
                            var schoolViewModel =
                                Assert.IsType<LocationSchoolOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(schoolMeta.Label, schoolViewModel.Label);
                            Assert.Equal(schoolMeta.Urn, schoolViewModel.Urn);
                            Assert.Equal(schoolMeta.LaEstab, schoolViewModel.LaEstab);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            Assert.Equal(dataSetVersion.LocationMetas.Select(lm => lm.Level), content.GeographicLevels);

            foreach (var indicator in content.Indicators)
            {
                var indicatorMeta = Assert.Single(dataSetVersion.IndicatorMetas, im => im.PublicId == indicator.Id);

                Assert.Equal(indicatorMeta.Label, indicator.Label);
                Assert.Equal(indicatorMeta.Unit, indicator.Unit);
                Assert.Equal(indicatorMeta.DecimalPlaces, indicator.DecimalPlaces);
            }

            foreach (var timePeriod in content.TimePeriods)
            {
                var timePeriodMeta = Assert.Single(
                    dataSetVersion.TimePeriodMetas,
                    tp => tp.Code == timePeriod.Code
                    && tp.Period == timePeriod.Period);

                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(timePeriodMeta.Period, timePeriodMeta.Code),
                    timePeriod.Label);
            }
        }

        [Fact]
        public async Task VersionSpecified_ReturnsCorrectVersion()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, dsv => dsv
                    .SetVersionNumber(1, 0)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 1")
                        ]
                    )
                )
                .ForIndex(1, dsv => dsv
                    .SetVersionNumber(1, 1)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 2")
                        ]
                    )
                )
                .ForIndex(2, dsv => dsv
                    .SetVersionNumber(2, 0)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 3")
                        ]
                    )
                )
                .ForIndex(3, dsv => dsv
                    .SetVersionNumber(2, 1)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 4")
                        ]
                    )
                )
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            var response = await GetDataSetMeta(dataSet.Id, "2.0");

            var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal("filter 3", content.Filters.Single().Label);
        }

        [Fact]
        public async Task VersionUnspecified_ReturnsLatestVersion()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, dsv => dsv
                    .SetVersionNumber(1, 0)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 1")
                        ]
                    )
                )
                .ForIndex(1, dsv => dsv
                    .SetVersionNumber(1, 1)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 2")
                        ]
                    )
                )
                .ForIndex(2, dsv => dsv
                    .SetVersionNumber(2, 0)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 3")
                        ]
                    )
                )
                .ForIndex(3, dsv => dsv
                    .SetVersionNumber(2, 1)
                    .SetFilterMetas(() =>
                        [
                            DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 4")
                        ]
                    )
                )
                .FinishWith(dsv => dataSet.LatestVersion = dsv)
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSetMeta(dataSet.Id);

            var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal("filter 4", content.Filters.Single().Label);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task VersionAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetMeta(dataSet.Id, dataSetVersion.Version);

            var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

            Assert.NotNull(content);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetMeta(dataSet.Id, dataSetVersion.Version);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionExistsForOtherDataSet_Returns404()
        {
            DataSet dataSet1 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet dataSet2 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet1.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetMeta(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSetMeta(dataSet.Id, "1.0");

            response.AssertNotFound();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await GetDataSetMeta(Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetMeta(Guid dataSetId, string? dataSetVersion = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "dataSetVersion", dataSetVersion?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/meta", query);

            var client = TestApp.CreateClient();

            return await client.GetAsync(uri);
        }
    }
}
