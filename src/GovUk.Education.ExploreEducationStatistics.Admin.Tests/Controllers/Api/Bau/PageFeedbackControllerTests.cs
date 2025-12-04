#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

// ReSharper disable once ClassNeverInstantiated.Global
public class PageFeedbackControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(PageFeedbackControllerTestsFixture))]
public class PageFeedbackControllerTestsCollection : ICollectionFixture<PageFeedbackControllerTestsFixture>;

[Collection(nameof(PageFeedbackControllerTestsFixture))]
public class PageFeedbackControllerTests(PageFeedbackControllerTestsFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private const string BaseUrl = "api/feedback/page";

    private readonly List<PageFeedback> _globalFeedback =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Created = Now,
            Response = PageFeedbackResponse.Useful,
            Url = "/",
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
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
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
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
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            Context = "What were you doing?",
            Intent = "",
            Issue = "What went wrong?",
            Read = false,
        },
    ];

    public async Task InitializeAsync()
    {
        // Arrange
        await fixture.GetContentDbContext().AddTestData(context => context.PageFeedback.AddRange(_globalFeedback));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListFeedback_NoQueryParameters_ReturnsOnlyUnreadFeedback()
    {
        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        var result = response.AssertOk<List<PageFeedbackViewModel>>();

        // Assert that the 2 pieces of expected unread feedback are returned.
        var unreadFeedback1 = result.Single(viewModel => viewModel.Id == _globalFeedback[0].Id);
        Assert.Equivalent(_globalFeedback[0], unreadFeedback1);

        var unreadFeedback2 = result.Single(viewModel => viewModel.Id == _globalFeedback[2].Id);
        Assert.Equivalent(_globalFeedback[2], unreadFeedback2);

        // Assert that the read piece of global feedback is not returned.
        Assert.Null(result.SingleOrDefault(viewModel => viewModel.Id == _globalFeedback[1].Id));
    }

    [Fact]
    public async Task ListFeedback_ShowRead_ReturnsAllFeedback()
    {
        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        // Act
        var response = await client.GetAsync(BaseUrl + "?showRead=true");

        // Assert
        var result = response.AssertOk<List<PageFeedbackViewModel>>();

        var globalFeedbackIds = _globalFeedback.Select(f => f.Id).ToList();
        var matchingGlobalFeedbackViewModels = result.Where(viewModel => globalFeedbackIds.Contains(viewModel.Id));
        Assert.Equal(3, matchingGlobalFeedbackViewModels.Count());
        Assert.Equivalent(_globalFeedback, matchingGlobalFeedbackViewModels);
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

        await fixture.GetContentDbContext().AddTestData(context => context.PageFeedback.Add(feedback));

        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        // Act
        await client.PatchAsync($"{BaseUrl}/{feedback.Id}", content: null);

        // Assert
        var saved = await fixture.GetContentDbContext().PageFeedback.SingleAsync(f => f.Id == feedback.Id);

        Assert.True(saved.Read);
    }
}
