using System.Net.Http.Json;
using System.Text;
using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Filters;

public class InvalidRequestInputResultFilterTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task TestPersonBody_Returns200()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsJsonAsync(
            requestUri: nameof(TestController.TestPersonBody),
            value: new TestPerson
            {
                Name = "Test name"
            });

        response.AssertOk();
    }

    [Theory]
    [InlineData("{}", "")]
    [InlineData("{ \"name\": 123 }", "name")]
    [InlineData("{ \"name\": true }", "name")]
    [InlineData("{ \"firstName\": \"test\" }", "")]
    [InlineData("{ \"firstName\": 123 }", "")]
    public async Task TestPersonBody_InvalidJson_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestPersonBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidInputError(path);

        Assert.Equal(ValidationMessages.InvalidInput.Message, error.Message);
    }

    [Fact]
    public async Task TestPersonBody_EmptyBody_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestPersonBody),
            content: new StringContent("", mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasNotEmptyBodyError();

        Assert.Equal(ValidationMessages.NotEmptyBody.Message, error.Message);
    }

    [Fact]
    public async Task TestPersonBody_RequiredField_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestPersonBody),
            content: new StringContent(
                "{ \"name\": null }",
                mediaType: "application/json",
                encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredFieldError("name");

        Assert.Equal(ValidationMessages.RequiredField.Message, error.Message);
    }

    [Theory]
    [InlineData("{}", "")]
    [InlineData("{ \"owner\": 123 }", "owner")]
    [InlineData("{ \"owner\": {} }", "owner")]
    [InlineData("{ \"owner\": { \"name\": 123 } }", "owner.name")]
    [InlineData("{ \"owner\": { \"name\": true } }", "owner.name")]
    [InlineData("{ \"owner\": { \"firstName\": \"test\" } }", "owner")]
    [InlineData("{ \"owner\": { \"firstName\": 123 } }", "owner")]
    public async Task TestGroupBody_InvalidJson_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidInputError(path);

        Assert.Equal(ValidationMessages.InvalidInput.Message, error.Message);
    }

    [Fact]
    public async Task TestGroupBody_EmptyBody_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent("", mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasNotEmptyBodyError();

        Assert.Equal(ValidationMessages.NotEmptyBody.Message, error.Message);
    }

    [Fact]
    public async Task TestGroupBody_RequiredField_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent(
                content: "{ \"owner\": { \"name\": null } }",
                mediaType: "application/json",
                encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredFieldError("owner.name");

        Assert.Equal(ValidationMessages.RequiredField.Message, error.Message);
    }

    [ApiController]
    private class TestController : ControllerBase
    {
        [HttpPost(nameof(TestPersonBody))]
        public ActionResult TestPersonBody([FromBody] TestPerson testPerson)
        {
            return Ok(testPerson);
        }

        [HttpPost(nameof(TestGroupBody))]
        public ActionResult TestGroupBody([FromBody] TestGroup testGroup)
        {
            return Ok(testGroup);
        }
    }

    private class TestPerson
    {
        public required string Name { get; init; }
    }

    private class TestGroup
    {
        public required TestPerson Owner { get; init; }
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp
            .WithWebHostBuilder(builder => builder.WithAdditionalControllers(typeof(TestController)))
            .ConfigureServices(s =>
            {
                s.Configure<ApiVersioningOptions>(options => options.AssumeDefaultVersionWhenUnspecified = true);
            });
    }
}
