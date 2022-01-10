#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    // ReSharper disable once UnusedType.Global
    public class PublicationNotifier
    {
        private const string SubscriptionsTblName = "Subscriptions";
        private const string PendingSubscriptionsTblName = "PendingSubscriptions";
        private const string StorageConnectionName = "TableStorageConnString";
        private const string NotifyApiKeyName = "NotifyApiKey";
        private const string BaseUrlName = "BaseUrl";
        private const string WebApplicationBaseUrlName = "WebApplicationBaseUrl";
        private const string TokenSecretKeyName = "TokenSecretKey";
        private const string ConfirmationEmailTemplateIdName = "SubscriptionConfirmationEmailTemplateId";
        private const string OriginalReleasePublicationEmailTemplateIdName =
            "OriginalReleasePublicationEmailTemplateId";
        private const string AmendedReleasePublicationEmailTemplateIdName =
            "AmendedReleasePublicationEmailTemplateId";
        private const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";

        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IStorageTableService _storageTableService;

        public PublicationNotifier(ITokenService tokenService, IEmailService emailService,
            IStorageTableService storageTableService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
        }

        [FunctionName("PublicationNotifier")]
        // ReSharper disable once UnusedMember.Global
        public void PublicationNotifierFunc(
            [QueueTrigger(PublicationQueue)] PublicationNotificationMessage notificationMessage,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation("{FunctionName} triggered",
                context.FunctionName);

            var config = LoadAppSettings(context);

            var emailTemplateId = notificationMessage.Amendment
                ? config.GetValue<string>(AmendedReleasePublicationEmailTemplateIdName)
                : config.GetValue<string>(OriginalReleasePublicationEmailTemplateIdName);

            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
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
                        { "update_note", notificationMessage.UpdateNote },
                        { "unsubscribe_link", baseUrl + entity.PartitionKey + "/unsubscribe/" + unsubscribeToken }
                    };

                    _emailService.SendEmail(client, entity.RowKey, emailTemplateId, values);
                }
            } while (token != null);
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

            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var subscriptionsTable = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
            var pendingSubscriptionsTable = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
            var client = GetNotifyClient(config);

            string? id = req.Query["id"];
            string? email = req.Query["email"];
            string? slug = req.Query["slug"];
            string? title = req.Query["title"];

            var subscriptionPending = false;
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

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
                subscriptionPending =
                    _storageTableService
                        .RetrieveSubscriber(pendingSubscriptionsTable, new SubscriptionEntity(id, email)).Result !=
                    null;

                logger.LogDebug("Pending subscription found?: {PendingSubscription}", subscriptionPending);

                // If already existing and pending then don't send another one
                if (!subscriptionPending)
                {
                    // Now check if already subscribed

                    var activeSubscriber = _storageTableService
                        .RetrieveSubscriber(subscriptionsTable, new SubscriptionEntity(id, email)).Result;
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
                                baseUrl + activeSubscriber.PartitionKey + "/unsubscribe/" + unsubscribeToken
                            }
                        };
                        _emailService.SendEmail(client, email, confirmationEmailTemplateId, confirmationValues);
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
                        { "verification_link", baseUrl + id + "/verify-subscription/" + activationCode }
                    };

                    _emailService.SendEmail(client, email, emailTemplateId, values);
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

            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            if (email != null)
            {
                var table = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
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

            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(ConfirmationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);
            var client = GetNotifyClient(config);

            if (email != null)
            {
                var pendingSubscriptionsTbl = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
                var subscriptionsTbl = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
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
                        { "unsubscribe_link", baseUrl + sub.PartitionKey + "/unsubscribe/" + unsubscribeToken }
                    };

                    _emailService.SendEmail(client, email, emailTemplateId, values);

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

            var config = LoadAppSettings(context);
            var pendingSubscriptionsTbl = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
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

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static NotificationClient GetNotifyClient(IConfiguration config)
        {
            var notifyApiKey = config.GetValue<string>(NotifyApiKeyName);
            return new NotificationClient(notifyApiKey);
        }

        private static CloudTable GetCloudTable(IStorageTableService storageTableService, IConfiguration config,
            string tableName)
        {
            var connectionStr = config.GetValue<string>(StorageConnectionName);
            return storageTableService.GetTable(connectionStr, tableName).Result;
        }
    }
}
