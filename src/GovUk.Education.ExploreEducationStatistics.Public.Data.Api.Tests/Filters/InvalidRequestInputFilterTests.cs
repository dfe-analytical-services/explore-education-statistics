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

public class InvalidRequestInputFilterTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task TestPersonBody_ValidBody_Returns200()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsJsonAsync(
            requestUri: nameof(TestController.TestPersonBody),
            value: new TestPerson
            {
                Name = "Test name"
            });

        var person = response.AssertOk<TestPerson>();

        Assert.Equal("Test name", person.Name);
    }

    [Theory]
    [InlineData("{}", "")]
    [InlineData("{ \"name\": 123 }", "name")]
    [InlineData("{ \"name\": true }", "name")]
    public async Task TestPersonBody_InvalidInput_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestPersonBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidInputError(path);

        Assert.Equal(ValidationMessages.InvalidValue.Message, error.Message);
    }

    [Theory]
    [InlineData("{ \"firstName\": \"test\" }", "firstName")]
    [InlineData("{ \"firstName\": 123 }", "firstName")]
    [InlineData("{ \"name\": \"Test\", \"lastName\": \"Last\"  }", "lastName")]
    public async Task TestPersonBody_UnknownFields_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestPersonBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasUnknownFieldError(path);

        Assert.Equal(ValidationMessages.UnknownField.Message, error.Message);
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
    public async Task TestPersonBody_RequiredValue_Returns400()
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

        var error = validationProblem.AssertHasRequiredValueError("name");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Fact]
    public async Task TestGroupBody_ValidBody_Returns200()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsJsonAsync(
            requestUri: nameof(TestController.TestGroupBody),
            value: new TestGroup
            {
                Owner = new TestPerson
                {
                    Name = "Test name"
                }
            });

        var group = response.AssertOk<TestGroup>();

        Assert.Equal("Test name", group.Owner.Name);
    }

    [Theory]
    [InlineData("{}", "")]
    [InlineData("{ \"owner\": 123 }", "owner")]
    [InlineData("{ \"owner\": {} }", "owner")]
    [InlineData("{ \"owner\": { \"name\": 123 } }", "owner.name")]
    [InlineData("{ \"owner\": { \"name\": true } }", "owner.name")]
    public async Task TestGroupBody_InvalidInput_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidInputError(path);

        Assert.Equal(ValidationMessages.InvalidValue.Message, error.Message);
    }

    [Theory]
    [InlineData("{ \"owner\": { \"firstName\": \"test\" } }", "owner.firstName")]
    [InlineData("{ \"owner\": { \"firstName\": 123 } }", "owner.firstName")]
    [InlineData("{ \"owner\": { \"name\": \"Test\", \"lastName\": \"Last\" } }", "owner.lastName")]
    public async Task TestGroupBody_InvalidJson_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasUnknownFieldError(path);

        Assert.Equal(ValidationMessages.UnknownField.Message, error.Message);
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
    public async Task TestGroupBody_RequiredValue_Returns400()
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

        var error = validationProblem.AssertHasRequiredValueError("owner.name");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?name=Test+name", "Test name")]
    [InlineData("?name=Test+name&age=30", "Test name", 30)]
    [InlineData("?name=123", "123")]
    [InlineData("?name=true", "true")]
    [InlineData("?name=", "")]
    public async Task TestPersonQuery_ValidQuery_Returns200(string query, string expectedName, int? expectedAge = null)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestPersonQuery)}{query}");

        var person = response.AssertOk<TestPerson>();

        Assert.Equal(expectedName, person.Name);
        Assert.Equal(expectedAge, person.Age);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?firstName=Test+name")]
    public async Task TestPersonQuery_RequiredValue_Returns400(string query)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestPersonQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredValueError("name");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?name=Test&age=ten")]
    public async Task TestPersonQuery_InvalidValue_Returns400(string query)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestPersonQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidValueError("age");

        Assert.Equal(ValidationMessages.InvalidValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?owner.name=Test+name", "Test name")]
    [InlineData("?owner.name=Test+name&owner.age=30", "Test name", 30)]
    [InlineData("?owner.name=123", "123")]
    [InlineData("?owner.name=true", "true")]
    [InlineData("?owner.name=", "")]
    public async Task TestGroupQuery_ValidQuery_Returns200(string query, string expectedName, int? expectedAge = null)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestGroupQuery)}{query}");

        var group = response.AssertOk<TestGroup>();

        Assert.Equal(expectedName, group.Owner.Name);
        Assert.Equal(expectedAge, group.Owner.Age);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?owner=Test")]
    [InlineData("?owner.namee=Test")]
    [InlineData("?owner.firstName=Test")]
    [InlineData("?name=Test")]
    public async Task TestGroupQuery_RequiredValue_Returns400(string query)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestGroupQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredValueError("owner");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?owner.name=Test&owner.age=ten")]
    public async Task TestGroupQuery_InvalidValue_Returns400(string query)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestGroupQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidValueError("owner.age");

        Assert.Equal(ValidationMessages.InvalidValue.Message, error.Message);
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

        [HttpGet(nameof(TestPersonQuery))]
        public ActionResult TestPersonQuery([FromQuery] TestPerson testPerson)
        {
            return Ok(testPerson);
        }

        [HttpGet(nameof(TestGroupQuery))]
        public ActionResult TestGroupQuery([FromQuery] TestGroup testGroup)
        {
            return Ok(testGroup);
        }
    }

    private class TestPerson
    {
        public required string Name { get; init; }

        public int? Age { get; init; }
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
