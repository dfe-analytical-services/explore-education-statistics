using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notify.Client;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using IConfigurationProvider = GovUk.Education.ExploreEducationStatistics.Notifier.Services.IConfigurationProvider;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions;

// ReSharper disable once UnusedType.Global
public class ReleaseNotifier
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IStorageTableService _storageTableService;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly INotificationClientProvider _notificationClientProvider;

    public ReleaseNotifier(
        ITokenService tokenService,
        IEmailService emailService,
        IStorageTableService storageTableService,
        IConfigurationProvider configurationProvider,
        INotificationClientProvider notificationClientProvider)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
        _configurationProvider = configurationProvider
                                 ?? throw new ArgumentNullException(nameof(configurationProvider));
        _notificationClientProvider = notificationClientProvider
                                 ?? throw new ArgumentNullException(nameof(notificationClientProvider));
    }

    [FunctionName("ReleaseNotifier")]
    // ReSharper disable once UnusedMember.Global
    public async Task ReleaseNotifierFunc(
        [QueueTrigger(ReleaseNotificationQueue)]
        ReleaseNotificationMessage msg,
        ILogger logger,
        ExecutionContext context)
    {
        logger.LogInformation("{FunctionName} triggered",
            context.FunctionName);

        var config = _configurationProvider.Get(context);

        var notifyApiKey = config.GetValue<string>(NotifyApiKeyName);
        var notificationClient = _notificationClientProvider.Get(notifyApiKey);

        var storageConnectionStr = config.GetValue<string>(StorageConnectionName);
        var subscribersTable = await _storageTableService.GetTable(storageConnectionStr, SubscriptionsTblName);

        var sentEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Send emails to subscribers of publication
        var releaseSubscriberQuery = new TableQuery<SubscriptionEntity>()
            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                msg.PublicationId.ToString()));
        var releaseSubscriberEmails = await GetSubscriberEmails(subscribersTable, releaseSubscriberQuery);

        foreach (var email in releaseSubscriberEmails)
        {
            SendSubscriberEmail(email, msg, notificationClient, config);
            sentEmails.Add(email);
        }

        logger.LogInformation("Emailed {NumReleaseSubscriberEmailsSent} publication subscribers",
            sentEmails.Count);

        // Send emails to subscribers of any associated superseded publication
        foreach (var supersededPublication in msg.SupersededPublications)
        {
            var releaseSupersededPubSubsQuery = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    supersededPublication.Id.ToString()));
            var supersededPublicationSubscriberEmails = await GetSubscriberEmails(
                subscribersTable, releaseSupersededPubSubsQuery);

            var numSupersededSubscriberEmailsSent = 0;

            foreach (var email in supersededPublicationSubscriberEmails)
            {
                if (sentEmails.Contains(email))
                {
                    continue;
                }

                SendSupersededSubscriberEmail(email, supersededPublication,
                    msg, notificationClient, config);
                sentEmails.Add(email);
                numSupersededSubscriberEmailsSent++;
            }
            logger.LogInformation("Emailed {NumSupersededPublicationEmailsSent} subscribers from a superseded publication",
                numSupersededSubscriberEmailsSent);
        }
        logger.LogInformation("Sent {TotalNumEmailsSent} emails in total to subscribers",
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
        ReleaseNotificationMessage msg,
        NotificationClient notificationClient,
        IConfigurationRoot config)
    {
        var baseUrl = config.GetValue<string>(BaseUrlName);
        var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
        var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

        var unsubscribeToken =
            _tokenService.GenerateToken(tokenSecretKey, email, DateTime.UtcNow.AddYears(1));

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", msg.PublicationName },
            { "release_name", msg.ReleaseName },
            {
                "release_link",
                $"{webApplicationBaseUrl}find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            { "unsubscribe_link", $"{baseUrl}{msg.PublicationId}/unsubscribe/{unsubscribeToken}" },
        };

        if (msg.Amendment)
        {
            values.Add("update_note", msg.UpdateNote);
        }

        var releaseTemplateId = msg.Amendment
            ? config.GetValue<string>(ReleaseAmendmentEmailTemplateIdName)
            : config.GetValue<string>(ReleaseEmailTemplateIdName);

        _emailService.SendEmail(notificationClient, email, releaseTemplateId, values);
    }

    private void SendSupersededSubscriberEmail(
        string email,
        IdTitleViewModel supersededPublication,
        ReleaseNotificationMessage msg,
        NotificationClient notificationClient,
        IConfigurationRoot config)
    {
        var baseUrl = config.GetValue<string>(BaseUrlName);
        var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
        var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

        var unsubscribeToken =
            _tokenService.GenerateToken(tokenSecretKey, email, DateTime.UtcNow.AddYears(1));

        var values = new Dictionary<string, dynamic>
        {
            { "publication_name", msg.PublicationName },
            { "release_name", msg.ReleaseName },
            {
                "release_link",
                $"{webApplicationBaseUrl}find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
            },
            { "unsubscribe_link", $"{baseUrl}{supersededPublication.Id}/unsubscribe/{unsubscribeToken}" },
            { "superseded_publication_title", supersededPublication.Title },
        };

        if (msg.Amendment)
        {
            values.Add("update_note", msg.UpdateNote);
        }

        var releaseSupersededSubscribersEmailTemplateId = msg.Amendment
            ? config.GetValue<string>(ReleaseAmendmentSupersededSubscribersEmailTemplateIdName)
            : config.GetValue<string>(ReleaseSupersededSubscribersEmailTemplateIdName);

        _emailService.SendEmail(notificationClient, email, releaseSupersededSubscribersEmailTemplateId, values);
    }
}
