using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests;

public class RequestTimeoutTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task TestGet()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: nameof(TestController.TestGet));

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPost()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(requestUri: nameof(TestController.TestPost), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPut()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PutAsync(requestUri: nameof(TestController.TestPut), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPatch()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PatchAsync(requestUri: nameof(TestController.TestPatch), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestDelete()
    {
        var client = BuildApp().CreateClient();

        var response = await client.DeleteAsync(requestUri: nameof(TestController.TestDelete));

        response.AssertGatewayTimeout();
    }

    [ApiController]
    private class TestController(IOptions<RequestTimeoutOptions> requestTimeoutOptions)
        : ControllerBase
    {
        [HttpGet(nameof(TestGet))]
        public async Task<ActionResult> TestGet(CancellationToken _)
        {
            await Task.Delay(_requestTimeout + 100, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpPost(nameof(TestPost))]
        public async Task<ActionResult> TestPost(CancellationToken _)
        {
            await Task.Delay(_requestTimeout + 100, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpPut(nameof(TestPut))]
        public async Task<ActionResult> TestPut(CancellationToken _)
        {
            await Task.Delay(_requestTimeout + 100, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpPatch(nameof(TestPatch))]
        public async Task<ActionResult> TestPatch(CancellationToken _)
        {
            await Task.Delay(_requestTimeout + 100, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpDelete(nameof(TestDelete))]
        public async Task<ActionResult> TestDelete(CancellationToken _)
        {
            await Task.Delay(_requestTimeout + 100, HttpContext.RequestAborted);
            return NoContent();
        }

        private readonly int _requestTimeout = requestTimeoutOptions.Value.TimeoutMilliseconds;
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp
            .WithWebHostBuilder(builder =>
            {
                builder.WithAdditionalControllers(typeof(TestController));
                builder.ConfigureAppConfiguration(
                    (_, config) =>
                        config.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                {
                                    $"{RequestTimeoutOptions.Section}:{nameof(RequestTimeoutOptions.TimeoutMilliseconds)}",
                                    "100"
                                },
                            }
                        )
                );
            })
            .ConfigureServices(s =>
            {
                s.Configure<ApiVersioningOptions>(options =>
                    options.AssumeDefaultVersionWhenUnspecified = true
                );
            });
    }
}
