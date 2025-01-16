#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class FeedbackControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/feedback";

    [Fact]
    public async Task CreateFeedback_Success()
    {
        // Arrange
        var client = TestApp.CreateClient();

        var request = new FeedbackCreateRequest
        {
            Response = FeedbackResponse.ProblemEncountered,
            Url = "/",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            Context = "What were you doing?",
            Intent = "What did you hope to achieve?",
            Issue = "What went wrong?",
        };

        // Act
        await client.PostAsync(BaseUrl, JsonContent.Create(request));

        // Assert
        await using var context = TestApp.GetDbContext<ContentDbContext>();
        var saved = await context.Feedback.FirstAsync();

        Assert.Equivalent(request, saved);
    }

    [Fact]
    public async Task FeedbackCreateRequestValidator_InvalidFields_IncludesRelevantErrors()
    {
        // Arrange
        var validator = new FeedbackCreateRequest.Validator();

        var request = new FeedbackCreateRequest
        {
            Response = (FeedbackResponse)5,
            Context = new string('a', 2001),
            Intent = new string('b', 2001),
            Issue = new string('c', 2001),
        };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        Assert.Equal(5, result.Errors.Count);
        result.ShouldHaveValidationErrorFor(feedback => feedback.Url);
        result.ShouldHaveValidationErrorFor(feedback => feedback.Response);
        result.ShouldHaveValidationErrorFor(feedback => feedback.Context);
        result.ShouldHaveValidationErrorFor(feedback => feedback.Intent);
        result.ShouldHaveValidationErrorFor(feedback => feedback.Issue);
    }
}
