#nullable enable
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class ReleasePublishingFeedbackControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/feedback/release-publishing";

    [Fact]
    public async Task UpdateFeedback_Success()
    {
        var existingFeedback = new ReleasePublishingFeedback
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            Role = PublicationRole.Owner,
            ReleaseVersionId = Guid.NewGuid(),
            Created = DateTime.UtcNow.AddDays(-1)
        };
        
        await TestApp.AddTestData<ContentDbContext>(context =>
            context.ReleasePublishingFeedback.Add(existingFeedback));

        var request = new ReleasePublishingFeedbackUpdateRequest(
            Token: existingFeedback.Token,
            Response: ReleasePublishingFeedbackResponse.Satisfied,
            AdditionalFeedback: "Great publishing experience!");
        
        // Arrange
        var client = TestApp.CreateClient();

        // Act
        var response = await client.PutAsync(BaseUrl, JsonContent.Create(request));
        response.AssertNoContent();

        // Assert
        await using var context = TestApp.GetDbContext<ContentDbContext>();
        var updatedFeedback = await context.ReleasePublishingFeedback.SingleAsync();

        // Assert that the fields that were expecting updates were updated successfully.
        Assert.Equal(request.Response, updatedFeedback.Response);
        Assert.Equal(request.AdditionalFeedback, updatedFeedback.AdditionalFeedback);
        updatedFeedback.FeedbackReceived.AssertUtcNow();
        
        // Assert that other fields were left untouched.
        Assert.Equal(existingFeedback.Token, updatedFeedback.Token);
        Assert.Equal(existingFeedback.Created, updatedFeedback.Created);
        Assert.Equal(existingFeedback.Role, updatedFeedback.Role);
        Assert.Equal(existingFeedback.ReleaseVersionId, updatedFeedback.ReleaseVersionId);
    }

    [Fact]
    public async Task UpdateFeedback_ValidationFailure_ReturnsBadRequest()
    {
        var existingFeedback = new ReleasePublishingFeedback
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            Role = PublicationRole.Owner,
            ReleaseVersionId = Guid.NewGuid(),
            Created = DateTime.UtcNow.AddDays(-1)
        };
        
        await TestApp.AddTestData<ContentDbContext>(context =>
            context.ReleasePublishingFeedback.Add(existingFeedback));

        var request = new ReleasePublishingFeedbackUpdateRequest(
            Token: "",
            Response: (ReleasePublishingFeedbackResponse) 20,
            AdditionalFeedback: new string('b', 2001));

        // Arrange
        var client = TestApp.CreateClient();

        // Act
        var response = await client.PutAsync(BaseUrl, JsonContent.Create(request));

        // Assert
        var validationProblems = response.AssertValidationProblem();
        Assert.Equal(3, validationProblems.Errors.Count);

        validationProblems.AssertHasNotEmptyError(
            nameof(ReleasePublishingFeedbackUpdateRequest.Token)
                .ToLowerFirst());
        validationProblems.AssertHasEnumError(
            nameof(ReleasePublishingFeedbackUpdateRequest.Response)
                .ToLowerFirst());
        validationProblems.AssertHasMaximumLengthError(
            nameof(ReleasePublishingFeedbackUpdateRequest.AdditionalFeedback)
                .ToLowerFirst(), maxLength: 2000);
    }
    
    [Fact]
    public async Task UpdateFeedback_UnknownToken_ReturnsNotFound()
    {
        var existingFeedback = new ReleasePublishingFeedback
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            Role = PublicationRole.Owner,
            ReleaseVersionId = Guid.NewGuid(),
            Created = DateTime.UtcNow.AddDays(-1)
        };
        
        await TestApp.AddTestData<ContentDbContext>(context =>
            context.ReleasePublishingFeedback.Add(existingFeedback));

        var request = new ReleasePublishingFeedbackUpdateRequest(
            Token: Guid.NewGuid().ToString(),
            Response: ReleasePublishingFeedbackResponse.Satisfied);

        // Arrange
        var client = TestApp.CreateClient();

        // Act
        var response = await client.PutAsync(BaseUrl, JsonContent.Create(request));

        // Assert
        response.AssertNotFound();
    }
}
