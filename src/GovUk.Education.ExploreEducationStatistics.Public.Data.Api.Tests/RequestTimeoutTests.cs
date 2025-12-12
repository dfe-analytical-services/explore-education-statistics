using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.Optimised;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class RequestTimeoutTestsFixture : OptimisedPublicApiCollectionFixture
{
    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        serviceModifications
            .AddController(typeof(TestController))
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { $"{RequestTimeoutOptions.Section}:{nameof(RequestTimeoutOptions.TimeoutMilliseconds)}", "100" },
                }
            )
            .ConfigureService<ApiVersioningOptions>(options => options.AssumeDefaultVersionWhenUnspecified = true);
    }
}

[CollectionDefinition(nameof(RequestTimeoutTestsFixture))]
public class RequestTimeoutTestsCollection : ICollectionFixture<RequestTimeoutTestsFixture>;

[Collection(nameof(RequestTimeoutTestsFixture))]
public class RequestTimeoutTests(RequestTimeoutTestsFixture fixture)
{
    [Fact]
    public async Task TestGet()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync(requestUri: nameof(TestController.TestGet));

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPost()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync(requestUri: nameof(TestController.TestPost), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPut()
    {
        var client = fixture.CreateClient();

        var response = await client.PutAsync(requestUri: nameof(TestController.TestPut), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestPatch()
    {
        var client = fixture.CreateClient();

        var response = await client.PatchAsync(requestUri: nameof(TestController.TestPatch), null);

        response.AssertGatewayTimeout();
    }

    [Fact]
    public async Task TestDelete()
    {
        var client = fixture.CreateClient();

        var response = await client.DeleteAsync(requestUri: nameof(TestController.TestDelete));

        response.AssertGatewayTimeout();
    }
}

[ApiController]
public class TestController(IOptions<RequestTimeoutOptions> requestTimeoutOptions) : ControllerBase
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
