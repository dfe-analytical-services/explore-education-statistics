#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.NotifierUtils;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    // ReSharper disable once UnusedType.Global
    public class ReleaseNotifier
    {
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ISubscriberService _subscriberService;

        public ReleaseNotifier(
            ITokenService tokenService,
            IEmailService emailService,
            ISubscriberService subscriberService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _subscriberService = subscriberService ?? throw new ArgumentNullException(nameof(subscriberService));
        }

        [FunctionName("ReleaseNotifier")]
        // ReSharper disable once UnusedMember.Global
        public async Task ReleaseNotifierFunc(
            [QueueTrigger(ReleaseNotificationQueue)]
            ReleaseNotificationMessage notificationMessage,
            ILogger logger,
            ExecutionContext context,
            CancellationToken cancellationToken = default)
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

            var subscribers = _subscriberService.RetrieveSubscribers(
                publicationId: notificationMessage.PublicationId,
                cancellationToken: cancellationToken);

            await foreach (var page in subscribers.AsPages().WithCancellation(cancellationToken))
            {
                foreach (var subscription in page.Values)
                {
                    var unsubscribeToken =
                        _tokenService.GenerateToken(tokenSecretKey, subscription.RowKey, DateTime.UtcNow.AddYears(1));
                    var values = new Dictionary<string, dynamic>
                    {
                        // Use values from the queue just in case the name or slug of the publication changes
                        { "publication_name", notificationMessage.PublicationName },
                        { "release_name", notificationMessage.ReleaseName },
                        {
                            "release_link",
                            $"{webApplicationBaseUrl}find-statistics/{notificationMessage.PublicationSlug}/{notificationMessage.ReleaseSlug}"
                        },
                        {
                            // NOTE: update_note not used by original release email template
                            "update_note", notificationMessage.UpdateNote
                        },
                        { "unsubscribe_link", $"{baseUrl}{subscription.PartitionKey}/unsubscribe/{unsubscribeToken}" },
                    };

                    _emailService.SendEmail(email: subscription.RowKey,
                        templateId: emailTemplateId,
                        values: values);
                }
            }
        }
    }
}
