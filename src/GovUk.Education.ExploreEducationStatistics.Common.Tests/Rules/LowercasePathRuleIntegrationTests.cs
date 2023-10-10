#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Rules;

public class LowercasePathRuleIntegrationTests
    : IClassFixture<TestApplicationFactory<TestStartup>>
{
    private readonly WebApplicationFactory<TestStartup> _testApp;

    public LowercasePathRuleIntegrationTests(TestApplicationFactory<TestStartup> testApp)
    {
        _testApp = testApp;
    }

    [Fact]
    public async Task PathIsLowercased_Get()
    {
        var client = SetupApp().CreateClient();

        var response = await client.GetAsync("api/test/Path?query=Query");

        // Client should follow 308 permanent redirect
        response.AssertOk(new TestResponse(Path: "path", Query: "Query"));
        response.AssertPathAndQueryEqualTo("/api/test/path?query=Query");
    }

    [Fact]
    public async Task PathIsLowercased_Post()
    {
        var client = SetupApp().CreateClient();

        var response = await client.PostAsync("api/test/Path?query=Query",
            content: new JsonNetContent("Body"));

        // Client should follow 308 permanent redirect without changing method from POST to GET
        response.AssertOk(new TestResponse(Path: "path", Query: "Query", Body: "Body"));
        response.AssertPathAndQueryEqualTo("/api/test/path?query=Query");
    }

    private WebApplicationFactory<TestStartup> SetupApp()
    {
        return _testApp.WithWebHostBuilder(builder => builder
            .WithAdditionalControllers(typeof(TestController)));
    }

    [ApiController]
    [Route("api/test/{path}")]
    private class TestController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<TestResponse> Get(
            string path,
            [FromQuery] string query)
        {
            return new TestResponse(path, query);
        }

        [HttpPost("")]
        public ActionResult<TestResponse> Post(
            string path,
            [FromQuery] string query,
            [FromBody, Required] string body)
        {
            return new TestResponse(path, query, body);
        }
    }

    private record TestResponse(string Path, string Query, string? Body = null);
}
