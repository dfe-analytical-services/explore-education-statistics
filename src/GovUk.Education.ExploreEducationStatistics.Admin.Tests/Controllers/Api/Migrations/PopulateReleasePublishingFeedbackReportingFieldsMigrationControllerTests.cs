using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Migrations;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Migrations;

public class PopulateReleasePublishingFeedbackReportingFieldsMigrationControllerTests
{
    private readonly DataFixture _fixture = new();
    
    [Fact]
    public async Task PopulateFields()
    {
        ReleaseVersion releaseVersion1 = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()));

        // This feedback entry has been responded to and is fully populated aside
        // from the fields that still need migrated.
        var releaseVersion1Feedback = new ReleasePublishingFeedback
        {
            Created = DateTime.UtcNow.AddDays(-2),
            EmailToken = Guid.NewGuid().ToString(),
            ReleaseVersion = releaseVersion1,
            ReleaseVersionId = releaseVersion1.Id,
            Response = ReleasePublishingFeedbackResponse.Satisfied,
            AdditionalFeedback = "Amazing experience!",
            FeedbackReceived = DateTime.UtcNow.AddDays(-1),
            UserPublicationRole = PublicationRole.Allower
        };

        ReleaseVersion releaseVersion2 = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture.DefaultPublication()));

        // This feedback entry has not yet been responded to.
        var releaseVersion2Feedback1 = new ReleasePublishingFeedback
        {
            ReleaseVersion = releaseVersion2,
            ReleaseVersionId = releaseVersion2.Id,
            EmailToken = Guid.NewGuid().ToString(),
            UserPublicationRole = PublicationRole.Owner
        };

        // This feedback entry has already been migrated.
        var releaseVersion2Feedback2 = new ReleasePublishingFeedback
        {
            ReleaseVersion = releaseVersion2,
            ReleaseVersionId = releaseVersion2.Id,
            EmailToken = Guid.NewGuid().ToString(),
            UserPublicationRole = PublicationRole.Owner,
            ReleaseTitle = releaseVersion2.Release.Title,
            PublicationTitle = releaseVersion2.Release.Publication.Title,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
            context.ReleasePublishingFeedback.AddRange(
                releaseVersion1Feedback, releaseVersion2Feedback1, releaseVersion2Feedback2);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var controller = new PopulateReleasePublishingFeedbackReportingFieldsMigrationController(context);
            var result = await controller.PopulateFields(cancellationToken: default);
            Assert.Equal(3, result.Processed);
        }
        
        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var feedbackEntries = context
                .ReleasePublishingFeedback
                .ToList();
            
            Assert.Equal(3, feedbackEntries.Count);
            var releaseVersion1FeedbackUpdated = feedbackEntries[0];
            var releaseVersion2Feedback1Updated = feedbackEntries[1];
            var releaseVersion2Feedback2Updated = feedbackEntries[2];

            AssertFeedbackEntryUpdateSuccessfully(
                originalFeedback: releaseVersion1Feedback,
                updatedFeedback: releaseVersion1FeedbackUpdated);
            
            AssertFeedbackEntryUpdateSuccessfully(
                originalFeedback: releaseVersion2Feedback1,
                updatedFeedback: releaseVersion2Feedback1Updated);
            
            AssertFeedbackEntryUpdateSuccessfully(
                originalFeedback: releaseVersion2Feedback2,
                updatedFeedback: releaseVersion2Feedback2Updated);
        }
    }

    private void AssertFeedbackEntryUpdateSuccessfully(
        ReleasePublishingFeedback originalFeedback,
        ReleasePublishingFeedback updatedFeedback)
    {
        // Assert that the fields which should have remained untouched have not been updated.
        Assert.Equal(originalFeedback.Id, updatedFeedback.Id);
        Assert.Equal(originalFeedback.Created, updatedFeedback.Created);
        Assert.Equal(originalFeedback.EmailToken, updatedFeedback.EmailToken);
        Assert.Equal(originalFeedback.ReleaseVersionId, updatedFeedback.ReleaseVersionId);
        Assert.Equal(originalFeedback.Response, updatedFeedback.Response);
        Assert.Equal(originalFeedback.AdditionalFeedback, updatedFeedback.AdditionalFeedback);
        Assert.Equal(originalFeedback.FeedbackReceived, updatedFeedback.FeedbackReceived);
        Assert.Equal(originalFeedback.UserPublicationRole, updatedFeedback.UserPublicationRole);

        // Assert that the migrated fields have been successfully filled in.
        Assert.Equal(originalFeedback.ReleaseVersion.Release.Title, updatedFeedback.ReleaseTitle);
        Assert.Equal(originalFeedback.ReleaseVersion.Release.Publication.Title, updatedFeedback.PublicationTitle);
    }
}
