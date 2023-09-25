#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.ModelBinding;

public class TrimStringModelBinderIntegrationTests
    : IClassFixture<TestApplicationFactory<TestStartup>>
{
    private readonly WebApplicationFactory<TestStartup> _testApp;

    public TrimStringModelBinderIntegrationTests(TestApplicationFactory<TestStartup> testApp)
    {
        _testApp = testApp;
    }

    [Fact]
    public async Task RouteValuesAreTrimmed()
    {
        var client = SetupApp().CreateClient();

        var response = await client.GetAsync("api/test/  path  ");

        response.AssertOk(new PathResponse("path"));
    }

    [Fact]
    public async Task QueryValuesAreTrimmed_SimpleType()
    {
        var client = SetupApp().CreateClient();

        var response = await client.GetAsync("api/test?query=  query  ");

        response.AssertOk(new QueryResponse("query"));
    }

    [Fact]
    public async Task QuerySourceValuesAreTrimmed_ComplexType()
    {
        var client = SetupApp().CreateClient();

        var response = await client.GetAsync("api/test/complex?request.query=  query  ");

        response.AssertOk(new QueryResponse("query"));
    }

    [Fact]
    public async Task FormValuesAreTrimmed_SimpleType()
    {
        var client = SetupApp().CreateClient();

        var response = await client.PostAsync("api/test",
            content: new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("query", "  query  ")
            }));

        response.AssertOk(new QueryResponse("query"));
    }

    [Fact]
    public async Task FormValuesAreTrimmed_ComplexType()
    {
        var client = SetupApp().CreateClient();

        var response = await client.PostAsync("api/test/complex",
            content: new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("request.query", "  query  ")
            }));

        response.AssertOk(new QueryResponse("query"));
    }

    private WebApplicationFactory<TestStartup> SetupApp()
    {
        return _testApp.WithWebHostBuilder(builder => builder
            .WithAdditionalControllers(typeof(TestController)));
    }

    [ApiController]
    [Route("api/test")]
    private class TestController : ControllerBase
    {
        [HttpGet("{path}")]
        public ActionResult<PathResponse> FromRoute([FromRoute] string path)
        {
            return new PathResponse(path);
        }

        [HttpGet("")]
        public ActionResult<QueryResponse> FromQuery([FromQuery] string query)
        {
            return new QueryResponse(query);
        }

        [HttpGet("complex")]
        public ActionResult<QueryResponse> FromQueryComplex([FromQuery] ComplexRequest request)
        {
            return new QueryResponse(request.Query);
        }

        [HttpPost("")]
        public ActionResult<QueryResponse> FromForm([FromForm] string query)
        {
            return new QueryResponse(query);
        }

        [HttpPost("complex")]
        public ActionResult<QueryResponse> FromFormComplex([FromForm] ComplexRequest request)
        {
            return new QueryResponse(request.Query);
        }
    }

    private record ComplexRequest(string Query);

    private record PathResponse(string Path);

    private record QueryResponse(string Query);
}
