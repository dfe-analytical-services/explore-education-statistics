using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetQueryControllerTests : IntegrationTestFixture
{
    public DataSetQueryControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    public class QueryDataSetGetTests : DataSetQueryControllerTests
    {
        public QueryDataSetGetTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Test()
        {
            var client = BuildApp().CreateClient();
            // var response = await client.GetAsync();
        }
        //
        // private Task<HttpResponse> QueryDataSet()
        // {
        //     // return client.GetAsync();
        // }

    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp;
    }
}
