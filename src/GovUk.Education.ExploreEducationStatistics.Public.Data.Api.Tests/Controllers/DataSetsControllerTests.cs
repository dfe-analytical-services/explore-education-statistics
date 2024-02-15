using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
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
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestVersion = dsv);

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
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                content.LatestVersion.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                content.LatestVersion.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, content.LatestVersion.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, content.LatestVersion.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, content.LatestVersion.Indicators);
        }

        [Theory]
        [InlineData(DataSetStatus.Staged)]
        [InlineData(DataSetStatus.Unpublished)]
        public async Task DataSetNotAvailable_Returns404(DataSetStatus dataSetStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            response.AssertNotFound();
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
        public async Task MultipleDataSetVersionsAvailableForRequestedDataSet_Returns200_PaginatedCorrectly(
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
        public async Task MultipleDataSetVersionsAvailableForRequestedDataSet_Returns200_OrderedCorrectly()
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
                .ForIndex(2, dsv => dsv.SetVersionNumber(1, 2))
                .ForIndex(3, dsv => dsv.SetVersionNumber(3, 5))
                .ForIndex(4, dsv => dsv.SetVersionNumber(3, 4))
                .ForIndex(5, dsv => dsv.SetVersionNumber(3, 3))
                .ForIndex(6, dsv => dsv.SetVersionNumber(3, 2))
                .ForIndex(7, dsv => dsv.SetVersionNumber(3, 1))
                .ForIndex(8, dsv => dsv.SetVersionNumber(3, 0))
                .ForIndex(9, dsv => dsv.SetVersionNumber(2, 0))
                .ForIndex(10, dsv => dsv.SetVersionNumber(2, 1))
                .ForIndex(11, dsv => dsv.SetVersionNumber(2, 2))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
            });

            var response = await ListVersions(
                dataSetId: dataSet.Id,
                page: 2,
                pageSize: 5);

            var content = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(2, content.Paging.Page);
            Assert.Equal(5, content.Paging.PageSize);
            Assert.Equal(12, content.Paging.TotalResults);
            Assert.Equal(5, content.Results.Count);

            Assert.Equal("3.0", content.Results[0].Number);
            Assert.Equal("2.2", content.Results[1].Number);
            Assert.Equal("2.1", content.Results[2].Number);
            Assert.Equal("2.0", content.Results[3].Number);
            Assert.Equal("1.2", content.Results[4].Number);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Unpublished)]
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

            DataSetVersion dataSetVersion = dataSetVersionStatus == DataSetVersionStatus.Unpublished
                ? dataSetVersionGenerator.WithUnpublished(DateTimeOffset.UtcNow)
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
            Assert.Equal(dataSetVersion.VersionType(), result.Type);
            Assert.Equal(dataSetVersion.Status, result.Status);
            Assert.Equal(
                dataSetVersion.Published!.Value.ToUnixTimeSeconds(),
                result.Published.ToUnixTimeSeconds()
            );
            Assert.Equal(
                dataSetVersion.Unpublished?.ToUnixTimeSeconds(),
                result.Unpublished?.ToUnixTimeSeconds()
            );
            Assert.Equal(dataSetVersion.Notes, result.Notes);
            Assert.Equal(dataSetVersion.TotalResults, result.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                result.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                result.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, result.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.Indicators);
        }

        [Fact]
        public async Task DataSetVersionAvailableForOtherDataSet_Returns200_OnlyVersionForRequestedDataSet()
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
        [InlineData(DataSetVersionStatus.Staged)]
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
}
