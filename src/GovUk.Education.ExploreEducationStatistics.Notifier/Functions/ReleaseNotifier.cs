using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ReleaseNotifier(
    ContentDbContext contentDbContext,
    ILogger<ReleaseNotifier> logger,
    IOptions<AppOptions> appOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    ISubscriptionRepository subscriptionRepository)
{
    private readonly AppOptions _appOptions = appOptions.Value;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;

    private static class FunctionNames
    {
        private const string Base = "PublicationSubscriptions_";
        public const string NotifySubscribers = $"{Base}{nameof(ReleaseNotifier.NotifySubscribers)}";
    }

    [Function(FunctionNames.NotifySubscribers)]
    public async Task NotifySubscribers(
        [QueueTrigger(NotifierQueueStorage.ReleaseNotificationQueue)] ReleaseNotificationMessage msg,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var sentEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // First send emails to publication subscribers
        {
            var subscriberEmails = await subscriptionRepository.GetSubscriberEmails(msg.PublicationId);

            foreach (var email in subscriberEmails)
            {
                SendSubscriberEmail(email, msg);
                sentEmails.Add(email);
            }
        }

        logger.LogInformation("Emailed {NumReleaseSubscriberEmailsSent} publication subscribers",
            sentEmails.Count);

        // Then send emails to subscribers of any associated superseded publication
        var supersededPublicationList = await contentDbContext.Publications
            .Where(p => p.SupersededById == msg.PublicationId)
            .ToListAsync();

        var numSupersededSubscriberEmailsSent = 0;
        foreach (var supersededPublication in supersededPublicationList)
        {
            var supersededPubSubEmails = await subscriptionRepository.GetSubscriberEmails(
                supersededPublication.Id);

            foreach (var email in supersededPubSubEmails)
            {
                if (sentEmails.Contains(email))
                {
                    continue;
                }

                SendSupersededSubscriberEmail(email,
                    supersededPublication,
                    msg);
                sentEmails.Add(email);
                numSupersededSubscriberEmailsSent++;
            }

            logger.LogInformation(
                "Emailed {NumSupersededPublicationEmailsSent} subscribers from a superseded publication",
                numSupersededSubscriberEmailsSent);
        }

        logger.LogInformation("Sent {TotalNumEmailsSent} emails in total to subscribers",
            sentEmails.Count);
    }

    private void SendSubscriberEmail(
        string email,
        ReleaseNotificationMessage msg)
    {
        var unsubscribeToken = tokenService.GenerateToken(email, DateTime.UtcNow.AddYears(1));

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", msg.PublicationName },
            { "release_name", msg.ReleaseName },
            {
                "release_link",
                $"{_appOptions.PublicAppUrl}/find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            {
                "unsubscribe_link",
                $"{_appOptions.PublicAppUrl}/subscriptions/{msg.PublicationSlug}/confirm-unsubscription/{unsubscribeToken}"
            }
        };

        if (msg.Amendment)
        {
            values.Add("update_note", msg.UpdateNote);
        }

        var releaseTemplateId = msg.Amendment
            ? _emailTemplateOptions.ReleaseAmendmentPublishedId
            : _emailTemplateOptions.ReleasePublishedId;

        emailService.SendEmail(
            email: email,
            templateId: releaseTemplateId,
            values);
    }

    private void SendSupersededSubscriberEmail(
        string email,
        Publication supersededPublication,
        ReleaseNotificationMessage msg)
    {
        var unsubscribeToken = tokenService.GenerateToken(email, DateTime.UtcNow.AddYears(1));

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", msg.PublicationName },
            { "release_name", msg.ReleaseName },
            {
                "release_link",
                $"{_appOptions.PublicAppUrl}/find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            {
                "unsubscribe_link",
                $"{_appOptions.PublicAppUrl}/subscriptions/{supersededPublication.Slug}/confirm-unsubscription/{unsubscribeToken}"
            },
            { "superseded_publication_title", supersededPublication.Title }
        };

        if (msg.Amendment)
        {
            values.Add("update_note", msg.UpdateNote);
        }

        var releaseSupersededSubscribersEmailTemplateId = msg.Amendment
            ? _emailTemplateOptions.ReleaseAmendmentPublishedSupersededSubscribersId
            : _emailTemplateOptions.ReleasePublishedSupersededSubscribersId;

        emailService.SendEmail(
            email: email,
            templateId: releaseSupersededSubscribersEmailTemplateId,
            values);
    }
}
