using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.WebUtilities;
using System.Drawing.Printing;

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

    public class ListVersionsTests : DataSetsControllerTests
    {
        public ListVersionsTests(TestApplicationFactory testApp) : base(testApp)
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

            var response = await ListVersions(
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

            var page1Response = await ListVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 3);

            var page1Content = page1Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.Equal(3, page1Content.Results.Count);
            Assert.Equal("3.1", page1Content.Results[0].Number);
            Assert.Equal("3.0", page1Content.Results[1].Number);
            Assert.Equal("2.1", page1Content.Results[2].Number);

            var page2Response = await ListVersions(
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

            var response = await ListVersions(
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
                result.Unpublished?.ToUnixTimeSeconds()
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

            var response = await ListVersions(
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

            var response = await ListVersions(
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

            var response = await ListVersions(
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

            var response = await ListVersions(
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
            var response = await ListVersions(
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
            var response = await ListVersions(
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

            var response = await ListVersions(dataSetId: dataSet.Id);

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

        private async Task<HttpResponseMessage> ListVersions(
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

    public class GetVersionTests : DataSetsControllerTests
    {
        public GetVersionTests(TestApplicationFactory testApp) : base(testApp)
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

            var response = await GetVersion(dataSet.Id, dataSetVersion.Version);

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
                content.Unpublished?.ToUnixTimeSeconds()
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

            var response = await GetVersion(dataSet.Id, dataSetVersion.Version);

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

            var response = await GetVersion(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetVersion(dataSet.Id, "1.0");

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

            var response = await GetVersion(Guid.NewGuid(), dataSetVersion.Version);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetVersion(Guid dataSetId, string dataSetVersion)
        {
            var client = TestApp.CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class GetMetaTests : DataSetsControllerTests
    {
        public GetMetaTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Unpublished)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task VersionIsAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var locationOptionMetaGeneratorByLevel = new Dictionary<GeographicLevel, Func<LocationOptionMeta>>
            {
                { GeographicLevel.School, () => DataFixture.DefaultLocationSchoolOptionMeta() },
                { GeographicLevel.LocalAuthority, () => DataFixture.DefaultLocationLocalAuthorityOptionMeta() },
                { GeographicLevel.RscRegion, () => DataFixture.DefaultLocationRscRegionOptionMeta() },
                { GeographicLevel.Provider, () => DataFixture.DefaultLocationProviderOptionMeta() },
                { GeographicLevel.EnglishDevolvedArea, () => DataFixture.DefaultLocationCodedOptionMeta() },
            };

            var filterMetas = DataFixture
                .DefaultFilterMeta()
                .GenerateList(3);

            foreach (var filterMeta in filterMetas)
            {
                var filterOptionMetas = DataFixture
                    .DefaultFilterOptionMeta()
                    .GenerateList(3);

                foreach (var filterOptionMeta in filterOptionMetas)
                {
                    var filterOptionMetaLink = DataFixture
                        .DefaultFilterOptionMetaLink()
                        .WithMeta(filterMeta)
                        .Generate();

                    filterOptionMeta.MetaLinks.Add(filterOptionMetaLink);
                }

                filterMeta.Options.AddRange(filterOptionMetas);
            }

            var locationMetas = new List<LocationMeta>();

            foreach (var locationOptionMetaGenerator in locationOptionMetaGeneratorByLevel)
            {
                LocationMeta locationMeta = DataFixture
                    .DefaultLocationMeta()
                    .WithLevel(locationOptionMetaGenerator.Key);

                var locationOptionMetas = new List<LocationOptionMeta>() 
                { 
                    locationOptionMetaGenerator.Value.Invoke(),
                    locationOptionMetaGenerator.Value.Invoke(),
                    locationOptionMetaGenerator.Value.Invoke(),
                };

                foreach (var locationOptionMeta in locationOptionMetas)
                {
                    var locationOptionMetaLink = DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithMeta(locationMeta)
                        .Generate();

                    locationOptionMeta.MetaLinks.Add(locationOptionMetaLink);
                }

                locationMeta.Options.AddRange(locationOptionMetas);

                locationMetas.Add(locationMeta);
            }

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
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

            var response = await GetMeta(dataSet.Id, dataSetVersion.Version);

            var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

            Assert.NotNull(content);

            foreach (var filterMetaViewModel in content.Filters)
            {
                var filterMeta = Assert.Single(dataSetVersion.FilterMetas, fm => fm.PublicId == filterMetaViewModel.Id);
                Assert.Equal(filterMeta.Hint, filterMetaViewModel.Hint);
                Assert.Equal(filterMeta.Label, filterMetaViewModel.Label);

                var allFilterMetaLinks = filterMeta.Options.SelectMany(fom => fom.MetaLinks);

                foreach (var filterOptionMetaViewModel in filterMetaViewModel.Options)
                {
                    var filterOptionMetaLink = Assert.Single(
                        allFilterMetaLinks, 
                        foml => SqidProcessor.Encode(foml.PublicId) == filterOptionMetaViewModel.Id);

                    var filterOptionMeta = Assert.Single(
                        filterMeta.Options,
                        fom => fom.Id == filterOptionMetaLink.OptionId);

                    Assert.Equal(filterOptionMeta.Label, filterOptionMetaViewModel.Label);
                    Assert.Equal(filterOptionMeta.IsAggregate, filterOptionMetaViewModel.IsAggregate);
                }
            }

            foreach (var locationMetaViewModel in content.Locations)
            {
                var locationMeta = Assert.Single(dataSetVersion.LocationMetas, fm => fm.Level == locationMetaViewModel.Level);

                var allLocationMetaLinks = locationMeta.Options.SelectMany(fom => fom.MetaLinks);

                foreach (var locationOptionMetaViewModel in locationMetaViewModel.Options)
                {
                    var locationOptionMetaLink = Assert.Single(
                        allLocationMetaLinks,
                        foml => SqidProcessor.Encode(foml.PublicId) == locationOptionMetaViewModel.Id);

                    var locationOptionMeta = Assert.Single(
                        locationMeta.Options,
                        fom => fom.Id == locationOptionMetaLink.OptionId);

                    switch (locationOptionMeta)
                    {
                        case LocationCodedOptionMeta locationCodedOptionMeta:
                            var locationCodedOptionMetaViewModel = Assert.IsType<LocationCodedOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(locationCodedOptionMeta.Label, locationCodedOptionMetaViewModel.Label);
                            Assert.Equal(locationCodedOptionMeta.Code, locationCodedOptionMetaViewModel.Code);
                            break;
                        case LocationLocalAuthorityOptionMeta locationLocalAuthorityOptionMeta:
                            var locationLocalAuthorityOptionMetaViewModel = Assert.IsType<LocationLocalAuthorityOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(locationLocalAuthorityOptionMeta.Label, locationLocalAuthorityOptionMetaViewModel.Label);
                            Assert.Equal(locationLocalAuthorityOptionMeta.Code, locationLocalAuthorityOptionMetaViewModel.Code);
                            Assert.Equal(locationLocalAuthorityOptionMeta.OldCode, locationLocalAuthorityOptionMetaViewModel.OldCode);
                            break;
                        case LocationProviderOptionMeta locationProviderOptionMeta:
                            var locationProviderOptionMetaViewModel = Assert.IsType<LocationProviderOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(locationProviderOptionMeta.Label, locationProviderOptionMetaViewModel.Label);
                            Assert.Equal(locationProviderOptionMeta.Ukprn, locationProviderOptionMetaViewModel.Ukprn);
                            break;
                        case LocationRscRegionOptionMeta locationRscRegionOptionMeta:
                            var locationRscRegionOptionMetaViewModel = Assert.IsType<LocationRscRegionOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(locationRscRegionOptionMeta.Label, locationRscRegionOptionMetaViewModel.Label);
                            break;
                        case LocationSchoolOptionMeta locationSchoolOptionMeta:
                            var locationSchoolOptionMetaViewModel = Assert.IsType<LocationSchoolOptionMetaViewModel>(locationOptionMetaViewModel);
                            Assert.Equal(locationSchoolOptionMeta.Label, locationSchoolOptionMetaViewModel.Label);
                            Assert.Equal(locationSchoolOptionMeta.Urn, locationSchoolOptionMetaViewModel.Urn);
                            Assert.Equal(locationSchoolOptionMeta.LaEstab, locationSchoolOptionMetaViewModel.LaEstab);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            foreach (var indicator in content.Indicators)
            {

            }
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Staged)]
        public async Task VersionNotAvailable_Returns404(DataSetVersionStatus dataSetVersionStatus)
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

            var response = await GetMeta(dataSet.Id, dataSetVersion.Version);

            response.AssertNotFound();
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

            var response = await GetMeta(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetMeta(dataSet.Id, "1.0");

            response.AssertNotFound();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await GetMeta(Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetMeta(Guid dataSetId, string? dataSetVersion = null)
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
