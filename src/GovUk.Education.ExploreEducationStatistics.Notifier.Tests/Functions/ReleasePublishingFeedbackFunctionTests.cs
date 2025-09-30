using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class ReleasePublishingFeedbackFunctionTests
{
    private static readonly DataFixture DataFixture = new();

    private static readonly AppOptions AppOptions = new() { PublicAppUrl = "https://public.app" };

    private static readonly GovUkNotifyOptions.EmailTemplateOptions EmailTemplateOptions = new()
    {
        ReleasePublishingFeedbackId = "release-publishing-feedback-id",
    };

    [Theory]
    [InlineData(PublicationRole.Owner, "an owner")]
    [InlineData(PublicationRole.Allower, "an approver")]
    public async Task SendReleasePublishingFeedbackEmail_Success(
        PublicationRole role,
        string expectedRoleDescription
    )
    {
        ReleaseVersion releaseVersion = DataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication())
            );

        var feedback = new ReleasePublishingFeedback
        {
            Id = Guid.NewGuid(),
            EmailToken = Guid.NewGuid().ToString(),
            UserPublicationRole = role,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseTitle = "Academic year 2022",
            PublicationTitle = "Publication title",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleasePublishingFeedback.Add(feedback);
            await context.SaveChangesAsync();
        }

        var releasePublishingFeedbackMessage = new ReleasePublishingFeedbackMessage(
            ReleasePublishingFeedbackId: feedback.Id,
            EmailAddress: "test@test.com"
        );

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                emailService: emailService.Object
            );

            await function.SendReleasePublishingFeedbackEmail(
                releasePublishingFeedbackMessage,
                cancellationToken: default
            );
        }

        emailService.Verify(
            s =>
                s.SendEmail(
                    releasePublishingFeedbackMessage.EmailAddress,
                    EmailTemplateOptions.ReleasePublishingFeedbackId,
                    It.Is<Dictionary<string, dynamic>>(values =>
                        AssertEmailTemplateValues(
                            values,
                            releaseVersion.Release.Publication.Title,
                            releaseVersion.Release.Title,
                            expectedRoleDescription,
                            feedback.EmailToken
                        )
                    )
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(PublicationRole.Drafter)]
    [InlineData(PublicationRole.Approver)]
    public async Task SendReleasePublishingFeedbackEmail_UnsupportedRoleUsed_NoEmailSent(
        PublicationRole role
    )
    {
        ReleaseVersion releaseVersion = DataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication())
            );

        var feedback = new ReleasePublishingFeedback
        {
            Id = Guid.NewGuid(),
            EmailToken = Guid.NewGuid().ToString(),
            UserPublicationRole = role,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseTitle = "Academic year 2022",
            PublicationTitle = "Publication title",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleasePublishingFeedback.Add(feedback);
            await context.SaveChangesAsync();
        }

        var releasePublishingFeedbackMessage = new ReleasePublishingFeedbackMessage(
            ReleasePublishingFeedbackId: feedback.Id,
            EmailAddress: "test@test.com"
        );

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                emailService: emailService.Object
            );

            await function.SendReleasePublishingFeedbackEmail(
                releasePublishingFeedbackMessage,
                cancellationToken: default
            );
        }

        emailService.Verify(
            s =>
                s.SendEmail(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>()
                ),
            Times.Never
        );
    }

    private static bool AssertEmailTemplateValues(
        Dictionary<string, dynamic> values,
        string publicationName,
        string releaseName,
        string roleDescription,
        string emailToken
    )
    {
        Assert.Equal(publicationName, values["publication_name"]);
        Assert.Equal(releaseName, values["release_name"]);
        Assert.Equal(roleDescription, values["role_description"]);
        Assert.Equal(
            $"{AppOptions.PublicAppUrl}/release-publishing-feedback?token={emailToken}",
            values["feedback_url"]
        );
        return true;
    }

    private static ReleasePublishingFeedbackFunction BuildFunction(
        ContentDbContext? contentDbContext = null,
        IEmailService? emailService = null
    )
    {
        return new ReleasePublishingFeedbackFunction(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            AppOptions.ToOptionsWrapper(),
            new GovUkNotifyOptions
            {
                ApiKey = "",
                EmailTemplates = EmailTemplateOptions,
            }.ToOptionsWrapper(),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            Mock.Of<ILogger<ReleasePublishingFeedbackFunction>>()
        );
    }
}
