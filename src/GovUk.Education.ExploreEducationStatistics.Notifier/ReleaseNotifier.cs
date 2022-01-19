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
            [QueueTrigger(ReleaseNotificationQueue)] ReleaseNotificationMessage notificationMessage,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation("{FunctionName} triggered",
                context.FunctionName);

            var config = LoadAppSettings(context);

            var emailTemplateId = notificationMessage.Amendment
                ? config.GetValue<string>(ReleaseAmendmentEmailTemplateIdName)
                : config.GetValue<string>(ReleaseEmailTemplateIdName);

            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var client = GetNotifyClient(config);
            var table = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    notificationMessage.PublicationId.ToString()));

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
                    var unsubscribeToken =
                        _tokenService.GenerateToken(tokenSecretKey, entity.RowKey, DateTime.UtcNow.AddYears(1));
                    var values = new Dictionary<string, dynamic>
                    {
                        // Use values from the queue just in case the name or slug of the publication changes
                        { "publication_name", notificationMessage.PublicationName },
                        { "release_name", notificationMessage.ReleaseName },
                        {
                            "release_link",
                            $"{webApplicationBaseUrl}find-statistics/{notificationMessage.PublicationSlug}/{notificationMessage.ReleaseSlug}"
                        },
                        { // NOTE: update_note not used by original release email template
                            "update_note", notificationMessage.UpdateNote
                        },
                        { "unsubscribe_link", $"{baseUrl}{entity.PartitionKey}/unsubscribe/{unsubscribeToken}" },
                    };

                    _emailService.SendEmail(client, entity.RowKey, emailTemplateId, values);
                }
            } while (token != null);
        }
    }
}
