#nullable enable
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Config;

public class FluentValidationActionFilterTests : IntegrationTest<TestStartup>
{
    private const string NotEmptyCode = "NotEmpty";
    private const string NotEmptyMessage = "Must not be empty.";

    private FluentValidationActionFilterTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    public class TestClassTests : FluentValidationActionFilterTests
    {
        public TestClassTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Body_Valid()
        {
            var body = new TestClass { Name = "Test name" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassBody), body);

            response.AssertOk();
        }

        [Fact]
        public async Task Body_Invalid()
        {
            var body = new TestClass { Name = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Single(errorDetail);
            Assert.Equal("", errorDetail["value"].GetString());
        }

        [Fact]
        public async Task BodyNested_Valid()
        {
            var body = new TestClass
            {
                Name = "Test name",
                TestAddresses = new List<TestAddress>
                {
                    new()
                    {
                        Line1 = "Test line 1"
                    }
                }
            };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassBody), body);

            response.AssertOk();
        }

        [Fact]
        public async Task BodyNested_Invalid()
        {
            var body = new TestClass
            {
                Name = "",
                TestAddresses = new List<TestAddress>
                {
                    new()
                    {
                        Line1 = ""
                    }
                }
            };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Equal(2, errors.Count);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var error0Detail = GetErrorDetail(errors[0]);

            Assert.Single(error0Detail);
            Assert.Equal("", error0Detail["value"].GetString());

            Assert.Equal("testAddresses[0].line1", errors[1].Path);
            Assert.Equal("MinimumLength", errors[1].Code);
            Assert.Equal("Must be at least 10 characters (was 0).", errors[1].Message);

            var error1Detail = GetErrorDetail(errors[1]);

            Assert.Equal(4, error1Detail.Count);
            Assert.Equal("", error1Detail["value"].GetString());
            Assert.Equal(10, error1Detail["minLength"].GetInt32());
            Assert.Equal(-1, error1Detail["maxLength"].GetInt32());
            Assert.Equal(0, error1Detail["totalLength"].GetInt32());
        }

        [Fact]
        public async Task Form_Valid()
        {
            var form = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "name", "Test name" },
                }
            );

            var client = BuildApp().CreateClient();
            var response = await client.PostAsync(nameof(TestController.TestClassForm), form);

            response.AssertOk();
        }

        [Fact]
        public async Task Form_Invalid()
        {
            var form = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "name", "" },
                }
            );

            var client = BuildApp().CreateClient();
            var response = await client.PostAsync(nameof(TestController.TestClassForm), form);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Single(errorDetail);
            Assert.Null(errorDetail["value"].GetString());
        }

        [Fact]
        public async Task Query_Valid()
        {
            var query = new Dictionary<string, string?>
            {
                { "name", "Test name" },
            };

            var client = BuildApp().CreateClient();
            var response = await client.GetAsync(
                QueryHelpers.AddQueryString(nameof(TestController.TestClassQuery), query));

            response.AssertOk();
        }

        [Fact]
        public async Task Query_Invalid()
        {
            var query = new Dictionary<string, string?>
            {
                { "name", "" },
            };

            var client = BuildApp().CreateClient();
            var response = await client.GetAsync(
                QueryHelpers.AddQueryString(nameof(TestController.TestClassQuery), query));

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Single(errorDetail);
            Assert.Null(errorDetail["value"].GetString());
        }

        [Fact]
        public async Task NoAttribute_Valid()
        {
            var body = new TestClass { Name = "Test name" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassNoAttribute), body);

            response.AssertOk();
        }

        [Fact]
        public async Task NoAttribute_Invalid()
        {
            var body = new TestClass { Name = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestClassNoAttribute), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Single(errorDetail);
            Assert.Equal("", errorDetail["value"].GetString());
        }
    }

    public class TestRecordTests : FluentValidationActionFilterTests
    {
        public TestRecordTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Valid()
        {
            var body = new TestRecord { Name = "Test name" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            response.AssertOk();
        }

        [Fact]
        public async Task Invalid()
        {
            var body = new TestRecord { Name = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("name", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Single(errorDetail);
            Assert.Equal("", errorDetail["value"].GetString());
        }

        [Fact]
        public async Task InvalidWithStatePrimitive()
        {
            var body = new TestRecord { Name = "Test", Person = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("person", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Equal(2, errorDetail.Count);
            Assert.Equal("", errorDetail["value"].GetString());
            Assert.Equal(1234, errorDetail["state"].GetInt32());
        }

        [Fact]
        public async Task InvalidWithStateObject()
        {
            var body = new TestRecord { Name = "Test", Role = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("role", errors[0].Path);
            Assert.Equal(NotEmptyCode, errors[0].Code);
            Assert.Equal(NotEmptyMessage, errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Equal(2, errorDetail.Count);
            Assert.Equal("", errorDetail["value"].GetString());
            Assert.Equal("Some allowed value", errorDetail["allowed"].GetString());
        }

        [Fact]
        public async Task InvalidWithPlaceholderValuesAndState()
        {
            var body = new TestRecord { Name = "Test", Location = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("location", errors[0].Path);
            Assert.Equal("MinimumLength", errors[0].Code);
            Assert.Equal("Must be at least 2 characters (was 0).", errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Equal(5, errorDetail.Count);
            Assert.Equal("", errorDetail["value"].GetString());
            Assert.Equal(2, errorDetail["minLength"].GetInt32());
            Assert.Equal(-1, errorDetail["maxLength"].GetInt32());
            Assert.Equal(0, errorDetail["totalLength"].GetInt32());
            Assert.Equal("Some reason", errorDetail["reason"].GetString());
        }

        [Fact]
        public async Task InvalidWithPlaceholderValuesAndOverridingState()
        {
            var body = new TestRecord { Name = "Test", Vehicle = "" };

            var client = BuildApp().CreateClient();
            var response = await client.PostAsJsonAsync(nameof(TestController.TestRecordBody), body);

            var details = response.AssertValidationProblem();

            var errors = details.Errors;

            Assert.Single(errors);

            Assert.Equal("vehicle", errors[0].Path);
            Assert.Equal("MinimumLength", errors[0].Code);
            Assert.Equal("Must be at least 2 characters (was 0).", errors[0].Message);

            var errorDetail = GetErrorDetail(errors[0]);

            Assert.Equal(4, errorDetail.Count);
            Assert.Equal("", errorDetail["value"].GetString());
            // Custom state overrides `minLength` and `maxLength`
            Assert.Equal(20, errorDetail["minLength"].GetInt32());
            Assert.Equal(100, errorDetail["maxLength"].GetInt32());
            Assert.Equal(0, errorDetail["totalLength"].GetInt32());
        }
    }

    private Dictionary<string, JsonElement> GetErrorDetail(ErrorViewModel error)
    {
        var errorDetailJson = Assert.IsType<JsonElement>(error.Detail);
        var errorDetail = errorDetailJson.Deserialize<Dictionary<string, JsonElement>>();

        Assert.NotNull(errorDetail);
        return errorDetail;
    }

    [ApiController]
    private class TestController : ControllerBase
    {
        [HttpPost(nameof(TestClassBody))]
        public ActionResult TestClassBody([FromBody] TestClass testClass)
        {
            return Ok(testClass.Name);
        }

        [HttpPost(nameof(TestClassForm))]
        public ActionResult TestClassForm([FromForm] TestClass testClass)
        {
            return Ok(testClass.Name);
        }

        [HttpGet(nameof(TestClassQuery))]
        public ActionResult TestClassQuery([FromQuery] TestClass testClass)
        {
            return Ok(testClass.Name);
        }

        [HttpPost(nameof(TestClassNoAttribute))]
        public ActionResult TestClassNoAttribute(TestClass testClass)
        {
            return Ok(testClass.Name);
        }

        [HttpPost(nameof(TestRecordBody))]
        public ActionResult TestRecordBody([FromBody] TestRecord testRecord)
        {
            return Ok(testRecord.Name);
        }
    }

    public class TestClass
    {
        public string? Name { get; init; }

        public List<TestAddress>? TestAddresses { get; init; }

        public class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
                When(
                    c => c.TestAddresses is not null,
                    () =>
                        RuleForEach(c => c.TestAddresses)
                            .SetValidator(new TestAddress.Validator())
                );
            }
        }
    }

    public class TestAddress
    {
        public string? Line1 { get; init; }

        public class Validator : AbstractValidator<TestAddress>
        {
            public Validator()
            {
                RuleFor(c => c.Line1).MinimumLength(10);
            }
        }
    }

    public class TestRecord
    {
        public string? Name { get; init; }

        public string? Role { get; init; }

        public string? Location { get; init; }

        public string? Person { get; init; }

        public string? Vehicle { get; init; }

        public class Validator : AbstractValidator<TestRecord>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
                When(
                    c => c.Role is not null,
                    () => RuleFor(c => c.Role)
                        .NotEmpty()
                        .WithState(_ => new
                        {
                            Allowed = "Some allowed value"
                        })
                );
                When(
                    c => c.Location is not null,
                    () => RuleFor(c => c.Location)
                        .MinimumLength(2)
                        .WithState(_ => new
                        {
                            Reason = "Some reason"
                        })
                );
                When(
                    c => c.Person is not null,
                    () => RuleFor(c => c.Person)
                        .NotEmpty()
                        .WithState(_ => 1234)
                );

                When(
                    c => c.Vehicle is not null,
                    () => RuleFor(c => c.Vehicle)
                        .MinimumLength(2)
                        .WithState(_ => new
                        {
                            MinLength = 20,
                            MaxLength = 100
                        })
                );
            }
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp()
    {
        return TestApp
            .WithWebHostBuilder(
                builder => { builder.WithAdditionalControllers(typeof(TestController)); }
            )
            .ConfigureServices(
                services => { services.AddFluentValidation(); }
            );
    }
}
