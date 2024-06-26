using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public class TestControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task Test()
    {
        DataSet dataSet = DataFixture.DefaultDataSet();

        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSets.Add(dataSet);
        });

        DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion()
            .WithFilterChanges(DataFixture)

        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            dataSetVersion.DataSet = dataSet;
            context.DataSetVersions.Add(dataSetVersion);
        });
    }
}
