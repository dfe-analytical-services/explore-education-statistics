using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

public class ReleaseNotifier
{
    private readonly ILogger<ReleaseNotifier> _logger;
    private readonly AppSettingsOptions _appSettingsOptions;
    private readonly GovUkNotifyOptions.EmailTemplateOptions _emailTemplateOptions;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IPublicationSubscriptionRepository _publicationSubscriptionRepository;

    public ReleaseNotifier(
        ILogger<ReleaseNotifier> logger,
        IOptions<AppSettingsOptions> appSettingsOptions,
        IOptions<GovUkNotifyOptions> govUkNotifyOptions,
        ITokenService tokenService,
        IEmailService emailService,
        IPublicationSubscriptionRepository publicationSubscriptionRepository)
    {
        _logger = logger;
        _appSettingsOptions = appSettingsOptions.Value;
        _emailTemplateOptions = govUkNotifyOptions.Value.EmailTemplates;
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _publicationSubscriptionRepository = publicationSubscriptionRepository ?? throw new ArgumentNullException(nameof(publicationSubscriptionRepository));
    }

    [Function("ReleaseNotifier")]
    public async Task ReleaseNotifierFunc(
        [QueueTrigger(ReleaseNotificationQueue)] ReleaseNotificationMessage msg,
        FunctionContext context)
    {
        _logger.LogInformation("{FunctionName} triggered", context.FunctionDefinition.Name);

        var subscribersTable = await _publicationSubscriptionRepository.GetTable(Constants.NotifierTableStorageTableNames.PublicationSubscriptionsTableName);

        var sentEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Send emails to subscribers of publication
        var releaseSubscriberQuery = new TableQuery<SubscriptionEntity>()
            .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal,
                msg.PublicationId.ToString()));
        var releaseSubscriberEmails = await GetSubscriberEmails(subscribersTable, releaseSubscriberQuery);

        foreach (var email in releaseSubscriberEmails)
        {
            SendSubscriberEmail(email, msg);
            sentEmails.Add(email);
        }

        _logger.LogInformation("Emailed {NumReleaseSubscriberEmailsSent} publication subscribers",
            sentEmails.Count);

        // Send emails to subscribers of any associated superseded publication
        foreach (var supersededPublication in msg.SupersededPublications)
        {
            var releaseSupersededPubSubsQuery = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    supersededPublication.Id.ToString()));
            var supersededPublicationSubscriberEmails = await GetSubscriberEmails(
                subscribersTable,
                releaseSupersededPubSubsQuery);

            var numSupersededSubscriberEmailsSent = 0;

            foreach (var email in supersededPublicationSubscriberEmails)
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

            _logger.LogInformation(
                "Emailed {NumSupersededPublicationEmailsSent} subscribers from a superseded publication",
                numSupersededSubscriberEmailsSent);
        }

        _logger.LogInformation("Sent {TotalNumEmailsSent} emails in total to subscribers",
            sentEmails.Count);
    }

    private static async Task<List<string>> GetSubscriberEmails(
        CloudTable table,
        TableQuery<SubscriptionEntity> query)
    {
        var emails = new List<string>();

        TableContinuationToken? token = null;
        do
        {
            var resultSegment =
                await table.ExecuteQuerySegmentedAsync(query, token);
            token = resultSegment.ContinuationToken;

            var newEmails = resultSegment.Results
                .Select(entity => entity.RowKey)
                .ToList();
            emails.AddRange(newEmails);
        } while (token != null);

        return emails;
    }

    private void SendSubscriberEmail(
        string email,
        ReleaseNotificationMessage msg)
    {
        var unsubscribeToken =
            _tokenService.GenerateToken(email, DateTime.UtcNow.AddYears(1));

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

        _emailService.SendEmail(
            email: email,
            templateId: releaseTemplateId,
            values);
    }

    private void SendSupersededSubscriberEmail(
        string email,
        IdTitleViewModel supersededPublication,
        ReleaseNotificationMessage msg)
    {
        var unsubscribeToken = _tokenService.GenerateToken(email, DateTime.UtcNow.AddYears(1));

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

        _emailService.SendEmail(
            email: email,
            templateId: releaseSupersededSubscribersEmailTemplateId,
            values);
    }
}
