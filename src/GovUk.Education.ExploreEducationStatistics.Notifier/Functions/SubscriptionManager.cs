using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notify.Exceptions;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using IConfigurationProvider = GovUk.Education.ExploreEducationStatistics.Notifier.Services.IConfigurationProvider;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Functions
{
    // ReSharper disable once UnusedType.Global
    public class SubscriptionManager
    {
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IStorageTableService _storageTableService;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly INotificationClientProvider _notificationClientProvider;

        public SubscriptionManager(
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
            _notificationClientProvider = notificationClientProvider ??
                                          throw new ArgumentNullException(nameof(notificationClientProvider));
        }

        [FunctionName("PublicationSubscribe")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]
            HttpRequest req,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = _configurationProvider.Get(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

            var storageConnectionStr = config.GetValue<string>(StorageConnectionName);
            var subscriptionsTable = await _storageTableService.GetTable(storageConnectionStr, SubscriptionsTblName);
            var pendingSubscriptionsTable = await _storageTableService.GetTable(storageConnectionStr, PendingSubscriptionsTblName);

            var notifyApiKey = config.GetValue<string>(NotifyApiKeyName);
            var notificationClient = _notificationClientProvider.Get(notifyApiKey);

            string? id = req.Query["id"];
            string? email = req.Query["email"];
            string? slug = req.Query["slug"];
            string? title = req.Query["title"];

            var subscriptionPending = false;
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
                var pendingSub =
                    await _storageTableService.RetrieveSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email));
                subscriptionPending = pendingSub != null;

                logger.LogDebug("Pending subscription found?: {PendingSubscription}", subscriptionPending);

                // If already existing and pending then don't send another one
                if (!subscriptionPending)
                {
                    // Now check if already subscribed

                    var activeSubscriber = await _storageTableService
                        .RetrieveSubscriber(subscriptionsTable, new SubscriptionEntity(id, email));
                    if (activeSubscriber != null)
                    {
                        var unsubscribeToken =
                            _tokenService.GenerateToken(tokenSecretKey, activeSubscriber.RowKey,
                                DateTime.UtcNow.AddYears(1));

                        var confirmationEmailTemplateId = config.GetValue<string>(ConfirmationEmailTemplateIdName);
                        var confirmationValues = new Dictionary<string, dynamic>
                        {
                            { "publication_name", activeSubscriber.Title },
                            {
                                "unsubscribe_link",
                                $"{baseUrl}{activeSubscriber.PartitionKey}/unsubscribe/{unsubscribeToken}"
                            }
                        };
                        _emailService.SendEmail(notificationClient, email, confirmationEmailTemplateId, confirmationValues);
                        return new OkResult();
                    }

                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = _tokenService.GenerateToken(tokenSecretKey, email, expiryDateTime);

                    await _storageTableService.UpdateSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email, title, slug, expiryDateTime));

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", title },
                        { "verification_link", $"{baseUrl}{id}/verify-subscription/{activationCode}" },
                    };

                    _emailService.SendEmail(notificationClient, email, emailTemplateId, values);
                }

                return new OkResult();
            }
            catch (NotifyClientException e)
            {
                logger.LogError(e, "Caught exception sending email");

                // Remove the subscriber from storage if we could not successfully send the email & just added it
                if (!subscriptionPending)
                {
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email));
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
            string token)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = _configurationProvider.Get(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            if (email != null)
            {
                var storageConnectionStr = config.GetValue<string>(StorageConnectionName);
                var table = await _storageTableService.GetTable(storageConnectionStr, SubscriptionsTblName);

                var sub = new SubscriptionEntity(id, email);
                sub = _storageTableService.RetrieveSubscriber(table, sub).Result;

                if (sub != null)
                {
                    await _storageTableService.RemoveSubscriber(table, sub);
                    return new RedirectResult(
                        webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&unsubscribed=true", true);
                }
            }

            return new BadRequestObjectResult("Unable to unsubscribe");
        }

        [FunctionName("PublicationSubscriptionVerify")]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PublicationSubscriptionVerifyFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]
            HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token)
        {
            logger.LogInformation("{FunctionName} triggered", context.FunctionName);

            var config = _configurationProvider.Get(context);
            var emailTemplateId = config.GetValue<string>(ConfirmationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName).AppendTrailingSlash();
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            var notifyApiKey = config.GetValue<string>(NotifyApiKeyName);
            var notificationClient = _notificationClientProvider.Get(notifyApiKey);

            if (email != null)
            {
                var storageConnectionStr = config.GetValue<string>(StorageConnectionName);
                var subscriptionsTbl = await _storageTableService.GetTable(storageConnectionStr, SubscriptionsTblName);
                var pendingSubscriptionsTbl = await _storageTableService.GetTable(storageConnectionStr, PendingSubscriptionsTblName);

                var sub = _storageTableService
                    .RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntity(id, email)).Result;

                if (sub != null)
                {
                    // Remove the pending subscription from the the file now verified
                    logger.LogDebug("Removing address from pending subscribers");
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, sub);

                    // Add them to the verified subscribers table
                    logger.LogDebug("Adding address to the verified subscribers");
                    sub.DateTimeCreated = DateTime.UtcNow;
                    await _storageTableService.UpdateSubscriber(subscriptionsTbl, sub);
                    var unsubscribeToken =
                        _tokenService.GenerateToken(tokenSecretKey, sub.RowKey, DateTime.UtcNow.AddYears(1));

                    var values = new Dictionary<string, dynamic>
                    {
                        { "publication_name", sub.Title },
                        { "unsubscribe_link", $"{baseUrl}{sub.PartitionKey}/unsubscribe/{unsubscribeToken}" }
                    };

                    _emailService.SendEmail(notificationClient, email, emailTemplateId, values);

                    return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&verified=true",
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
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                context.FunctionName,
                DateTime.UtcNow);

            var config = _configurationProvider.Get(context);

            var storageConnectionStr = config.GetValue<string>(StorageConnectionName);
            var pendingSubscriptionsTbl = await _storageTableService.GetTable(storageConnectionStr, PendingSubscriptionsTblName);

            // Remove any pending subscriptions where the token has expired i.e. more than 1 hour old
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("DateTimeCreated", QueryComparisons.LessThan,
                    DateTime.UtcNow.AddHours(1)));

            TableContinuationToken? token = null;
            do
            {
                var resultSegment = await pendingSubscriptionsTbl.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, entity);
                }
            } while (token != null);
        }
    }
}