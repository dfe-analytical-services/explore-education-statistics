using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

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
}
