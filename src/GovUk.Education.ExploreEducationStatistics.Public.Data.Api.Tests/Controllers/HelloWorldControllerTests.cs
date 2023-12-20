using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public class HelloWorldControllerTests : IntegrationTestFixture
{
    public HelloWorldControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    [Fact]
    public async Task Test()
    {
        var client = TestApp.CreateClient();

        var response = await client.GetAsync("/api/v1/HelloWorld");

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal("Hello World", content);
    }
}
