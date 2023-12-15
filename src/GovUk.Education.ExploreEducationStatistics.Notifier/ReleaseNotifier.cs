#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.NotifierUtils;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    // ReSharper disable once UnusedType.Global
    public class ReleaseNotifier
    {
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IStorageTableService _storageTableService;

        public ReleaseNotifier(
            ITokenService tokenService,
            IEmailService emailService,
            IStorageTableService storageTableService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
        }

        [FunctionName("ReleaseNotifier")]
        // ReSharper disable once UnusedMember.Global
        public void ReleaseNotifierFunc(
            [QueueTrigger(ReleaseNotificationQueue)]
            ReleaseNotificationMessage msg,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation("{FunctionName} triggered",
                context.FunctionName);

            var config = LoadAppSettings(context);

            var newReleaseTemplateId = msg.Amendment
                ? config.GetValue<string>(ReleaseAmendmentEmailTemplateIdName)
                : config.GetValue<string>(ReleaseEmailTemplateIdName);

            var newReleaseSupersededSubscribersTemplateId = msg.Amendment
                ? config.GetValue<string>(ReleaseAmendmentSupersededSubscribersEmailTemplateIdName)
                : config.GetValue<string>(ReleaseEmailSupersededSubscribersTemplateIdName);

            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var client = GetNotifyClient(config);
            var table = GetCloudTable(_storageTableService, config, SubscriptionsTblName);

            var sentToEmailAddresses = new HashSet<string>();

            // Send emails to subscribers of publication
            {
                var query = new TableQuery<SubscriptionEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                        msg.PublicationId.ToString()));

                TableContinuationToken? token = null;
                do
                {
                    var resultSegment =
                        table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = resultSegment.ContinuationToken;

                    logger.LogInformation("Emailing {SubscriberCount} subscribers",
                        resultSegment.Results.Count);
                    foreach (var entity in resultSegment.Results)
                    {
                        var email = entity.RowKey;

                        sentToEmailAddresses.Add(email);

                        var unsubscribeToken =
                            _tokenService.GenerateToken(tokenSecretKey, email, DateTime.UtcNow.AddYears(1));
                        var values = new Dictionary<string, dynamic>
                        {
                            // Use values from the queue just in case the name or slug of the publication changes
                            { "publication_name", msg.PublicationName },
                            { "release_name", msg.ReleaseName },
                            {
                                "release_link",
                                $"{webApplicationBaseUrl}find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
                            },
                            {
                                // NOTE: update_note not used by original release email template
                                "update_note", msg.UpdateNote
                            },
                            { "unsubscribe_link", $"{baseUrl}{msg.PublicationId}/unsubscribe/{unsubscribeToken}" },
                        };

                        _emailService.SendEmail(client, email, newReleaseTemplateId, values);
                    }
                } while (token != null);
            }

            // Send emails to subscribers of superseded publications - should only be 1 superseded publication, but
            // service allows multiple
            for (var i = 0; i < msg.SupersededPublicationIds.Count; i++)
            {
                var supersededPublicationId = msg.SupersededPublicationIds[i];
                var supersededPublicationTitle = msg.SupersededPublicationTitles[i];

                var query = new TableQuery<SubscriptionEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                        supersededPublicationId.ToString()));

                logger.LogInformation("Emailing subscribers from a superseded publication");
                var numSupersedeSubscriberEmailsSent = 0;

                TableContinuationToken? token = null;
                do
                {
                    var resultSegment =
                        table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = resultSegment.ContinuationToken;

                    numSupersedeSubscriberEmailsSent += resultSegment.Results.Count;
                    foreach (var entity in resultSegment.Results)
                    {
                        var email = entity.RowKey;

                        if (sentToEmailAddresses.Contains(email))
                        {
                            numSupersedeSubscriberEmailsSent--;
                            continue;
                        }
                        sentToEmailAddresses.Add(email);

                        var unsubscribeToken =
                            _tokenService.GenerateToken(tokenSecretKey, email, DateTime.UtcNow.AddYears(1));
                        var values = new Dictionary<string, dynamic>
                        {
                            // Use values from the queue just in case the name or slug of the publication changes
                            { "publication_name", msg.PublicationName },
                            { "release_name", msg.ReleaseName },
                            {
                                "release_link",
                                $"{webApplicationBaseUrl}find-statistics/{msg.PublicationSlug}/{msg.ReleaseSlug}"
                            },
                            {
                                // NOTE: update_note not used by original release email template
                                "update_note", msg.UpdateNote
                            },
                            { "unsubscribe_link", $"{baseUrl}{supersededPublicationId}/unsubscribe/{unsubscribeToken}" },
                            { "superseded_publication_title", supersededPublicationTitle },
                        };

                        _emailService.SendEmail(client, email, newReleaseSupersededSubscribersTemplateId, values);
                    }
                } while (token != null);

                logger.LogInformation("Emailed {NumEmailsSent} subscribers from a superseded publication",
                    numSupersedeSubscriberEmailsSent);
            }

            logger.LogInformation("Sent {NumEmailsSent} emails in total to subscribers",
                sentToEmailAddresses.Count);
        }
    }
}
