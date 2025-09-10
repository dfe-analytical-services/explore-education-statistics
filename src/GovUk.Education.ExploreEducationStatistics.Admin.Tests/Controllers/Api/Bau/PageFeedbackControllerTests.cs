#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public class PageFeedbackControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private const string BaseUrl = "api/feedback/page";
    private readonly List<PageFeedback> _feedback =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Created = Now,
            Response = PageFeedbackResponse.Useful,
            Url = "/",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            Context = "",
            Intent = "",
            Issue = "",
            Read = false,
        },
        new()
        {
            Id = Guid.NewGuid(),
            Created = Now,
            Response = PageFeedbackResponse.NotUseful,
            Url = "/",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            Context = "What were you doing?",
            Intent = "What did you hope to achieve?",
            Issue = "",
            Read = true,
        },
        new()
        {
            Id = Guid.NewGuid(),
            Created = Now,
            Response = PageFeedbackResponse.ProblemEncountered,
            Url = "/",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            Context = "What were you doing?",
            Intent = "",
            Issue = "What went wrong?",
            Read = false,
        }
    ];

    [Fact]
    public async Task ListFeedback_NoQueryParameters_ReturnsOnlyUnreadFeedback()
    {
        // Arrange
        await TestApp.AddTestData<ContentDbContext>(context
            => context.PageFeedback.AddRange(_feedback));

        var client = TestApp
            .SetUser(DataFixture.BauUser())
            .CreateClient();

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        var result = response.AssertOk<List<PageFeedbackViewModel>>();

        Assert.Equal(2, result.Count);
        Assert.Equivalent(_feedback[0], result[0]);
        Assert.Equivalent(_feedback[2], result[1]);
    }

    [Fact]
    public async Task ListFeedback_ShowRead_ReturnsAllFeedback()
    {
        // Arrange
        await TestApp.AddTestData<ContentDbContext>(context
            => context.PageFeedback.AddRange(_feedback));

        var client = TestApp
            .SetUser(DataFixture.BauUser())
            .CreateClient();

        // Act
        var response = await client.GetAsync(BaseUrl + "?showRead=true");

        // Assert
        var result = response.AssertOk<List<PageFeedbackViewModel>>();

        Assert.Equal(3, result.Count);
        Assert.Equivalent(_feedback, result);
    }

    [Fact]
    public async Task ToggleReadStatus_Success()
    {
        // Arrange
        var feedback = new PageFeedback
        {
            Id = Guid.NewGuid(),
            Response = PageFeedbackResponse.Useful,
            Url = "/",
            Read = false,
        };

        await TestApp.AddTestData<ContentDbContext>(context
            => context.PageFeedback.Add(feedback));

        await using var context = TestApp.GetDbContext<ContentDbContext>();

        var client = TestApp
            .SetUser(DataFixture.BauUser())
            .CreateClient();

        // Act
        await client.PatchAsync($"{BaseUrl}/{feedback.Id}", content: null);

        // Assert
        var saved = await context.PageFeedback.FirstAsync();

        Assert.True(saved.Read);
    }
}
