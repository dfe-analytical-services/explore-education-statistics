using System.Net.Http.Json;
using System.Text;
using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
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
            value: new TestPerson { Name = "Test name" }
        );

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
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
            content: new StringContent("", mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
            content: new StringContent("{ \"name\": null }", mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
            value: new TestGroup { Owner = new TestPerson { Name = "Test name" } }
        );

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
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8)
        );

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasInvalidInputError(path);

        Assert.Equal(ValidationMessages.InvalidValue.Message, error.Message);
    }

    [Theory]
    [InlineData("{ \"owner\": { \"firstName\": \"test\" } }", "owner.firstName")]
    [InlineData("{ \"owner\": { \"firstName\": 123 } }", "owner.firstName")]
    [InlineData("{ \"owner\": { \"name\": \"Test\", \"lastName\": \"Last\" } }", "owner.lastName")]
    public async Task TestGroupBody_UnknownFields_Returns400(string body, string path)
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync(
            requestUri: nameof(TestController.TestGroupBody),
            content: new StringContent(body, mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
            content: new StringContent("", mediaType: "application/json", encoding: Encoding.UTF8)
        );

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
                encoding: Encoding.UTF8
            )
        );

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
    [InlineData("?name=Test&aliases[0]=Alias1&aliases[1]=Alias2", "Test", null, "Alias1", "Alias2")]
    [InlineData("?name=Test&aliases=Alias1,Alias2", "Test", null, "Alias1", "Alias2")]
    public async Task TestPersonQuery_ValidQuery_Returns200(
        string query,
        string expectedName,
        int? expectedAge = null,
        params string[] expectedAliases
    )
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestPersonQuery)}{query}");

        var person = response.AssertOk<TestPerson>();

        Assert.Equal(expectedName, person.Name);
        Assert.Equal(expectedAge, person.Age);
        Assert.Equal(expectedAliases, person.Aliases);
    }

    [Fact]
    public async Task TestPersonQuery_RequiredValue_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: nameof(TestController.TestPersonQuery));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredValueError("name");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?name=Test&firstName=First", "firstName")]
    [InlineData("?name=Test&firstName=First&lastName=Last", "firstName", "lastName")]
    public async Task TestPersonQuery_UnknownFields_Returns400(string query, params string[] unknownFields)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestPersonQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Equal(unknownFields.Length, validationProblem.Errors.Count);

        Assert.All(
            unknownFields,
            field =>
            {
                var error = validationProblem.AssertHasUnknownFieldError(field);

                Assert.Equal(ValidationMessages.UnknownField.Message, error.Message);
            }
        );
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
    [InlineData("?owner.name=Test&owner.aliases[0]=Alias1&owner.aliases[1]=Alias2", "Test", null, "Alias1", "Alias2")]
    [InlineData("?owner.name=Test&owner.aliases=Alias1,Alias2", "Test", null, "Alias1", "Alias2")]
    public async Task TestGroupQuery_ValidQuery_Returns200(
        string query,
        string expectedName,
        int? expectedAge = null,
        params string[] expectedAliases
    )
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestGroupQuery)}{query}");

        var group = response.AssertOk<TestGroup>();

        Assert.Equal(expectedName, group.Owner.Name);
        Assert.Equal(expectedAge, group.Owner.Age);
        Assert.Equal(expectedAliases, group.Owner.Aliases);
    }

    [Fact]
    public async Task TestGroupQuery_RequiredValue_Returns400()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: nameof(TestController.TestGroupQuery));

        var validationProblem = response.AssertValidationProblem();

        Assert.Single(validationProblem.Errors);

        var error = validationProblem.AssertHasRequiredValueError("owner");

        Assert.Equal(ValidationMessages.RequiredValue.Message, error.Message);
    }

    [Theory]
    [InlineData("?owner.name=Test&owner=Owner", "owner")]
    [InlineData("?owner.name=Test&owner.firstName=First", "owner.firstName")]
    [InlineData("?owner.name=Test&owner.firstName=First&owner.lastName=Last", "owner.firstName", "owner.lastName")]
    [InlineData("?owner.name=Test&name=Test", "name")]
    public async Task TestGroupQuery_UnknownFields_Returns400(string query, params string[] unknownFields)
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestGroupQuery)}{query}");

        var validationProblem = response.AssertValidationProblem();

        Assert.Equal(unknownFields.Length, validationProblem.Errors.Count);

        Assert.All(
            unknownFields,
            field =>
            {
                var error = validationProblem.AssertHasUnknownFieldError(field);

                Assert.Equal(ValidationMessages.UnknownField.Message, error.Message);
            }
        );
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

    [Theory]
    [InlineData("?name=Test", "Test")]
    [InlineData("?name=Test&age=30", "Test", 30)]
    [InlineData("?name=Test&aliases[0]=Alias1&aliases[1]=Alias2", "Test", null, "Alias1", "Alias2")]
    public async Task TestQueryArgs_ValidQuery_Returns200(
        string query,
        string expectedName,
        int? expectedAge = null,
        params string[] expectedAliases
    )
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(requestUri: $"{nameof(TestController.TestQueryArgs)}{query}");

        var group = response.AssertOk<TestPerson>();

        Assert.Equal(expectedName, group.Name);
        Assert.Equal(expectedAge, group.Age);
        Assert.Equal(expectedAliases, group.Aliases);
    }

    [ApiController]
    private class TestController : ControllerBase
    {
        [HttpPost(nameof(TestPersonBody))]
        public ActionResult TestPersonBody([FromBody] TestPerson testPerson, CancellationToken _)
        {
            return Ok(testPerson);
        }

        [HttpPost(nameof(TestGroupBody))]
        public ActionResult TestGroupBody([FromBody] TestGroup testGroup, CancellationToken _)
        {
            return Ok(testGroup);
        }

        [HttpGet(nameof(TestPersonQuery))]
        public ActionResult TestPersonQuery([FromQuery] TestPerson testPerson, CancellationToken _)
        {
            return Ok(testPerson);
        }

        [HttpGet(nameof(TestGroupQuery))]
        public ActionResult TestGroupQuery([FromQuery] TestGroup testGroup, CancellationToken _)
        {
            return Ok(testGroup);
        }

        [HttpGet(nameof(TestQueryArgs))]
        public ActionResult TestQueryArgs(
            string name,
            [FromQuery(Name = "age")] int? ageNumber,
            [FromQuery] List<string>? aliases,
            CancellationToken _
        )
        {
            return Ok(
                new TestPerson
                {
                    Name = name,
                    Age = ageNumber,
                    Aliases = aliases ?? [],
                }
            );
        }
    }

    private class TestPerson
    {
        public required string Name { get; init; }

        public int? Age { get; init; }

        [FromQuery]
        [QuerySeparator]
        public List<string> Aliases { get; init; } = [];
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
