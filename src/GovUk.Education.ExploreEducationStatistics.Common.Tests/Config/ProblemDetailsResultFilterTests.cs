#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Config;

public class ProblemDetailsResultFilterTests(TestApplicationFactory<TestStartup> testApp)
    : IntegrationTest<TestStartup>(testApp)
{
    [Fact]
    public async Task Forbid_Returns403ProblemDetails()
    {
        var client = BuildApp().CreateClient();
        var response = await client.GetAsync(nameof(TestController.TestForbid));

        response.AssertForbidden();

        var problemDetails = response.AssertBodyIsProblemDetails();

        Assert.Equal(403, problemDetails.Status);
        Assert.Equal("Forbidden", problemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
    }

    [Fact]
    public async Task BadRequest_Returns400ProblemDetails()
    {
        var client = BuildApp().CreateClient();
        var response = await client.GetAsync(nameof(TestController.TestBadRequest));

        var problemDetails = response.AssertBadRequest();

        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problemDetails.Type);
    }

    [Fact]
    public async Task ServerError_Returns500ProblemDetails()
    {
        var client = BuildApp().CreateClient();
        var response = await client.GetAsync(nameof(TestController.TestServerError));

        var problemDetails = response.AssertInternalServerError();

        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problemDetails.Type);
    }

    [Fact]
    public async Task ValidationProblemWithModelState_Returns400ValidationProblem()
    {
        var client = BuildApp().CreateClient();
        var response = await client.GetAsync(nameof(TestController.TestValidationProblemWithModelState));

        var validationProblem = response.AssertValidationProblem();

        Assert.Equal(400, validationProblem.Status);
        Assert.Equal("One or more validation errors occurred.", validationProblem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", validationProblem.Type);

        var errors = validationProblem.Errors;

        Assert.Equal(2, errors.Count);

        Assert.Null(errors[0].Path);
        Assert.Equal("A global error", errors[0].Message);

        Assert.Equal("Test", errors[1].Path);
        Assert.Equal("A field error", errors[1].Message);
    }

    [Fact]
    public async Task ValidationProblemWithAttributes_Returns400ValidationProblem()
    {
        var client = BuildApp().CreateClient();
        var response = await client.PostAsJsonAsync(
            nameof(TestController.TestValidationProblemWithAttributes), new TestClass());

        var validationProblem = response.AssertValidationProblem();

        Assert.Equal(400, validationProblem.Status);
        Assert.Equal("One or more validation errors occurred.", validationProblem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", validationProblem.Type);

        var errors = validationProblem.Errors;

        Assert.Single(errors);

        Assert.Equal("Name", errors[0].Path);
        Assert.Equal("The Name field is required.", errors[0].Message);
    }

    [Fact]
    public async Task ValidationResult_Returns400ValidationProblem()
    {
        var client = BuildApp().CreateClient();
        var response = await client.GetAsync(nameof(TestController.TestValidationResult));

        var validationProblem = response.AssertValidationProblem();

        Assert.Equal(400, validationProblem.Status);
        Assert.Equal("One or more validation errors occurred.", validationProblem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", validationProblem.Type);

        var errors = validationProblem.Errors;

        Assert.Single(errors);

        Assert.Null(errors[0].Path);
        Assert.Equal("A global error", errors[0].Message);
    }

    [ApiController]
    private class TestController : ControllerBase
    {
        [HttpGet(nameof(TestForbid))]
        public ActionResult TestForbid()
        {
            return Forbid();
        }

        [HttpGet(nameof(TestBadRequest))]
        public ActionResult TestBadRequest()
        {
            return BadRequest();
        }

        [HttpGet(nameof(TestServerError))]
        public ActionResult TestServerError()
        {
            return StatusCode(500);
        }

        [HttpPost(nameof(TestValidationProblemWithAttributes))]
        public ActionResult TestValidationProblemWithAttributes(TestClass testClass)
        {
            return Ok(testClass.Name);
        }

        [HttpGet(nameof(TestValidationProblemWithModelState))]
        public ActionResult TestValidationProblemWithModelState()
        {
            ModelState.AddModelError("", "A global error");
            ModelState.AddModelError("Test", "A field error");

            return ValidationProblem();
        }

        [HttpGet(nameof(TestValidationResult))]
        public ActionResult TestValidationResult()
        {
            return ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Message = "A global error"
            });
        }
    }

    private class TestClass
    {
        [Required]
        public string? Name { get; init; }
    }

    private WebApplicationFactory<TestStartup> BuildApp()
    {
        return TestApp
            .WithWebHostBuilder(
                builder => { builder.WithAdditionalControllers(typeof(TestController)); }
            );
    }
}
