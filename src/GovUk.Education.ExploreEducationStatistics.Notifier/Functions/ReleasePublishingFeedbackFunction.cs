using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ReleasePublishingFeedbackFunction(
    ContentDbContext contentDbContext,
    IOptions<AppOptions> appOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    IEmailService emailService,
    ILogger<ReleasePublishingFeedbackFunction> logger)
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;

    [Function(nameof(SendReleasePublishingFeedbackEmail))]
    public async Task SendReleasePublishingFeedbackEmail(
        [QueueTrigger(NotifierQueueStorage.ReleasePublishingFeedbackQueue)] ReleasePublishingFeedbackMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await contentDbContext
                .ReleasePublishingFeedback
                .Include(feedback => feedback.ReleaseVersion)
                .ThenInclude(releaseVersion => releaseVersion.Release)
                .ThenInclude(release => release.Publication)
                .SingleOrNotFoundAsync(
                    feedback => feedback.Id == message.ReleasePublishingFeedbackId,
                    cancellationToken)
                .OnSuccessDo(feedback =>
                {
                    var release = feedback.ReleaseVersion.Release;

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", release.Publication.Title },
                        { "release_name", release.Title },
                        { "role_description", GetRoleDescription(feedback.UserPublicationRole) },
                        { "feedback_url", $"{_appOptions.PublicAppUrl}/release-publishing-feedback?token={feedback.EmailToken}" }
                    };

                    emailService.SendEmail(
                        email: message.EmailAddress,
                        templateId: _emailTemplateOptions.ReleasePublishingFeedbackId,
                        values);
                })
                .OrThrow(_ => new Exception("Unable to send release publishing feedback " +
                                            $"for {message.ReleasePublishingFeedbackId}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured while executing '{FunctionName}'", nameof(SendReleasePublishingFeedbackEmail));
        }
    }

    private static string GetRoleDescription(PublicationRole role)
    {
        return role switch
        {
            PublicationRole.Owner => "an owner",
            PublicationRole.Allower => "an approver",

            // Note that this function should never be invoked for PublicationRole.Approver
            // or PublicationRole.Drafter currently, because Publisher is filtering
            // these out until the permissions simplification work has been completed.
            PublicationRole.Drafter => throw new ArgumentException(
                $"{nameof(ReleasePublishingFeedbackFunction)} should not " +
                $"have been called for {nameof(PublicationRole.Drafter)}"),
            PublicationRole.Approver => throw new ArgumentException(
                $"{nameof(ReleasePublishingFeedbackFunction)} should not " +
                $"have been called for {nameof(PublicationRole.Approver)}"),

            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
