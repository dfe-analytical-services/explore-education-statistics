#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class PageFeedbackControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/feedback/page";

    [Fact]
    public async Task CreateFeedback_Success()
    {
        // Arrange
        var client = TestApp.CreateClient();

        var request = new PageFeedbackCreateRequest
        {
            Response = PageFeedbackResponse.ProblemEncountered,
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
        var saved = await context.PageFeedback.FirstAsync();

        Assert.Equivalent(request, saved);
    }

    [Fact]
    public async Task CreateFeedback_ValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var client = TestApp.CreateClient();

        var request = new PageFeedbackCreateRequest
        {
            UserAgent = new string('a', 251),
            Response = (PageFeedbackResponse)5,
            Context = new string('b', 2001),
            Intent = new string('c', 2001),
            Issue = new string('d', 2001),
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(request));

        // Assert
        var validationProblems = response.AssertValidationProblem();
        Assert.Equal(6, validationProblems.Errors.Count);

        validationProblems.AssertHasNotEmptyError(nameof(PageFeedbackCreateRequest.Url).ToLowerFirst());
        validationProblems.AssertHasMaximumLengthError(nameof(PageFeedbackCreateRequest.UserAgent).ToLowerFirst(), maxLength: 250);
        validationProblems.AssertHasEnumError(nameof(PageFeedbackCreateRequest.Response).ToLowerFirst());
        validationProblems.AssertHasMaximumLengthError(nameof(PageFeedbackCreateRequest.Context).ToLowerFirst(), maxLength: 2000);
        validationProblems.AssertHasMaximumLengthError(nameof(PageFeedbackCreateRequest.Intent).ToLowerFirst(), maxLength: 2000);
        validationProblems.AssertHasMaximumLengthError(nameof(PageFeedbackCreateRequest.Issue).ToLowerFirst(), maxLength: 2000);
    }
}
