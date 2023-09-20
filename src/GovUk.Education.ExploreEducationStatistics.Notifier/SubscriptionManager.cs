#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notify.Exceptions;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.NotifierUtils;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    // ReSharper disable once UnusedType.Global
    public class SubscriptionManager
    {
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ISubscriberService _subscriberService;

        public SubscriptionManager(ITokenService tokenService,
            IEmailService emailService,
            ISubscriberService subscriberService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _subscriberService = subscriberService ?? throw new ArgumentNullException(nameof(subscriberService));
        }

        [FunctionName("PublicationSubscribe")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]
            HttpRequest req,
            ILogger logger,
            ExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

            string? id = req.Query["id"];
            string? email = req.Query["email"];
            string? slug = req.Query["slug"];
            string? title = req.Query["title"];

            var data = await req.GetJsonBody();

            id ??= data?.id;
            email ??= data?.email;
            slug ??= data?.slug;
            title ??= data?.title;

            if (id == null || email == null || slug == null || title == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            try
            {
                var pendingSubscriber = await _subscriberService.RetrievePendingSubscriber(publicationId: id,
                            email: email,
                            cancellationToken: cancellationToken);

                logger.LogDebug("Pending subscription found?: {PendingSubscription}", pendingSubscriber != null);

                // If already existing and pending then don't send another one
                if (pendingSubscriber != null)
                {
                    // Now check if already subscribed

                    var subscriber = await _subscriberService.RetrieveSubscriber(publicationId: id,
                        email: email,
                        cancellationToken: cancellationToken);

                    if (subscriber != null)
                    {
                        var unsubscribeToken =
                            _tokenService.GenerateToken(tokenSecretKey, subscriber.RowKey,
                                DateTime.UtcNow.AddYears(1));

                        var confirmationEmailTemplateId = config.GetValue<string>(ConfirmationEmailTemplateIdName);
                        var confirmationValues = new Dictionary<string, dynamic>
                        {
                            { "publication_name", subscriber.Title },
                            {
                                "unsubscribe_link",
                                $"{baseUrl}{subscriber.PartitionKey}/unsubscribe/{unsubscribeToken}"
                            }
                        };

                        _emailService.SendEmail(email: email,
                            templateId: confirmationEmailTemplateId,
                            values: confirmationValues);

                        return new OkResult();
                    }

                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = _tokenService.GenerateToken(tokenSecretKey, email, expiryDateTime);

                    await _subscriberService.CreateOrUpdateSubscriber(
                        SubscriptionTableNames.PendingSubscriptionsTableName,
                        new SubscriptionEntity(id, email, title, slug, expiryDateTime),
                        cancellationToken: cancellationToken);

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", title },
                        { "verification_link", $"{baseUrl}{id}/verify-subscription/{activationCode}" },
                    };

                    _emailService.SendEmail(email: email,
                        templateId: emailTemplateId,
                        values: values);
                }

                return new OkResult();
            }
            catch (NotifyClientException e)
            {
                logger.LogError(e, "Caught exception sending email");

                // Remove the subscriber from storage if we could not successfully send the email & just added it
                if (!subscriptionPending)
                {
                    await _subscriberService.RemovePendingSubscriber(publicationId: id,
                        email: email,
                        cancellationToken: cancellationToken);
                }

                return new BadRequestResult();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Caught exception storing subscription");
                return new BadRequestResult();
            }
        }

        [FunctionName("PublicationUnsubscribe")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationUnsubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]
            HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            if (email != null)
            {
                var subscriber = await _subscriberService.RetrieveSubscriber(publicationId: id,
                    email: email,
                    cancellationToken: cancellationToken);

                if (subscriber != null)
                {
                    await _subscriberService.RemoveSubscriber(publicationId: id,
                        email: email,
                        cancellationToken: cancellationToken);

                    return new RedirectResult(
                        webApplicationBaseUrl + $"subscriptions?slug={subscriber.Slug}&unsubscribed=true", true);
                }
            }

            return new BadRequestObjectResult("Unable to unsubscribe");
        }

        [FunctionName("PublicationSubscriptionVerify")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscriptionVerifyFunc(
            [HttpTrigger(authLevel: AuthorizationLevel.Anonymous,
                methods: "get",
                Route = "publication/{publicationId}/verify-subscription/{token}")]
            HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context,
            string publicationId,
            string token,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(ConfirmationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            if (email != null)
            {
                var pendingSubscriber = await _subscriberService.RetrievePendingSubscriber(publicationId: publicationId,
                    email: email,
                    cancellationToken: cancellationToken);

                if (pendingSubscriber != null)
                {
                    // Remove the pending subscription
                    logger.LogDebug("Removing address from pending subscribers");
                    await _subscriberService.RemovePendingSubscriber(publicationId: publicationId,
                        email: email,
                        cancellationToken: cancellationToken);

                    // Add a verified subscription
                    logger.LogDebug("Adding address to the verified subscribers");
                    pendingSubscriber.DateTimeCreated = DateTime.UtcNow;
                    await _subscriberService.CreateOrUpdateSubscriber(SubscriptionTableNames.SubscriptionsTableName, 
                        pendingSubscriber,
                        cancellationToken: cancellationToken);

                    var unsubscribeToken =
                        _tokenService.GenerateToken(tokenSecretKey, pendingSubscriber.RowKey, DateTime.UtcNow.AddYears(1));

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", pendingSubscriber.Title },
                        { "unsubscribe_link", $"{baseUrl}{pendingSubscriber.PartitionKey}/unsubscribe/{unsubscribeToken}" }
                    };

                    _emailService.SendEmail(email: email,
                        templateId: emailTemplateId,
                        values: values);

                    return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={pendingSubscriber.Slug}&verified=true",
                        true);
                }
            }

            return new BadRequestObjectResult("Unable to verify subscription");
        }

        [FunctionName("RemoveNonVerifiedSubscriptions")]
        // ReSharper disable once UnusedMember.Global
        public async Task RemoveNonVerifiedSubscriptionsFunc(
            [TimerTrigger("0 0 * * * *")] TimerInfo myTimer,
            ExecutionContext context,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                context.FunctionName,
                DateTime.UtcNow);

            await _subscriberService.RemoveExpiredPendingSubscriptions(cancellationToken: cancellationToken);
        }
    }
}
