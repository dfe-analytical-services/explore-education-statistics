using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ReleaseNotifier(
    ILogger<ReleaseNotifier> logger,
    IOptions<AppSettingsOptions> appSettingsOptions,
    IOptions<GovUkNotifyOptions> govUkNotifyOptions,
    ITokenService tokenService,
    IEmailService emailService,
    IApiSubscriptionTableStorageService apiSubscriptionTableStorageService)
{
    private readonly AppSettingsOptions _appSettingsOptions = appSettingsOptions.Value;
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
        // @MarkFix check this
        {
            // @MarkFix could create a repository to avoid all this...
            var publicationIdString = msg.PublicationId.ToString(); // To make the unit test mocks work
            var results = await apiSubscriptionTableStorageService
                .QueryEntities<SubscriptionEntity>(
                    tableName: NotifierTableStorage.PublicationSubscriptionsTable,
                    filter: sub => sub.PartitionKey == publicationIdString,
                    select: [nameof(SubscriptionEntity.RowKey)]); // email address

            // @MarkFix especially check this - we used to fetch in segments (I think)
            var releaseSubscribers = await results.ToListAsync();

            foreach (var subscription in releaseSubscribers)
            {
                var email = subscription.RowKey;

                SendSubscriberEmail(email, msg);
                sentEmails.Add(email);
            }
        }

        logger.LogInformation("Emailed {NumReleaseSubscriberEmailsSent} publication subscribers",
            sentEmails.Count);

        // Then send emails to subscribers of any associated superseded publication
        var numSupersededSubscriberEmailsSent = 0;
        foreach (var supersededPublication in msg.SupersededPublications)
        {
            var supersededPublicationIdString = supersededPublication.Id.ToString(); // To make unit test mocks work
            var results = await apiSubscriptionTableStorageService.QueryEntities<SubscriptionEntity>(
                tableName: NotifierTableStorage.PublicationSubscriptionsTable,
                filter: sub => sub.PartitionKey == supersededPublicationIdString,
                select: [ nameof(SubscriptionEntity.RowKey) ]); // email address

            var supersededPubSubs = await results.ToListAsync();

            foreach (var subscription in supersededPubSubs)
            {
                var email = subscription.RowKey;

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
                $"{_appSettingsOptions.PublicAppUrl}/find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            {
                "unsubscribe_link",
                $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{msg.PublicationSlug}/confirm-unsubscription/{unsubscribeToken}"
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
        IdTitleViewModel supersededPublication,
        ReleaseNotificationMessage msg)
    {
        var unsubscribeToken = tokenService.GenerateToken(email, DateTime.UtcNow.AddYears(1));

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", msg.PublicationName },
            { "release_name", msg.ReleaseName },
            {
                "release_link",
                $"{_appSettingsOptions.PublicAppUrl}/find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            {
                "unsubscribe_link",
                $"{_appSettingsOptions.PublicAppUrl}/subscriptions/{msg.PublicationSlug}/confirm-unsubscription/{unsubscribeToken}"
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
