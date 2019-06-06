using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Notify.Client;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class PublicationNotifier
    {
        private const string SubscriptionsTblName = "Subscriptions";
        private const string PendingSubscriptionsTblName = "PendingSubscriptions";
        private const string StorageConnectionName = "TableStorageConnString";
        private const string NotifyApiKeyName = "NotifyApiKey";
        private const string BaseUrlName = "BaseUrl";
        private const string WebApplicationBaseUrlName = "WebApplicationBaseUrl";
        private const string TokenSecretKeyName = "TokenSecretKey";
        private const string NotificationEmailTemplateIdName = "PublicationNotificationEmailTemplateId";
        private const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";

        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IStorageTableService _storageTableService;

        public PublicationNotifier(ITokenService tokenService, IEmailService emailService, IStorageTableService storageTableService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _storageTableService = storageTableService ?? throw new ArgumentNullException(nameof(storageTableService));
        }

        [FunctionName("PublicationNotifier")]
        public void PublicationNotifierFunc(
            [QueueTrigger("publication-queue", Connection = "")]
            JObject pn,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation($"PublicationNotifier function triggered : {pn.ToString()}");

            var publicationNotification = ExtractNotification(pn);
            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(NotificationEmailTemplateIdName);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var client = GetNotifyClient(config);
            var table = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, publicationNotification.PublicationId));

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SubscriptionEntity> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (SubscriptionEntity entity in resultSegment.Results)
                {
                    logger.LogInformation($"There are {resultSegment.Results.Count} subscribed to this publication");
                    var unsubscribeToken = _tokenService.GenerateToken(tokenSecretKey, entity.RowKey, DateTime.UtcNow.AddYears(1));
                    var vals = new Dictionary<string, dynamic>
                       {
                           // Use values from the queue just in case the name or slug of the publication chnages
                           {"publication_name", publicationNotification.Name},
                           {"publication_link", webApplicationBaseUrl + "statistics/" + publicationNotification.Slug},
                           {"unsubscribe_link", baseUrl + entity.PartitionKey + "/unsubscribe/" + unsubscribeToken}
                       };

                    _emailService.sendEmail(client, entity.RowKey, emailTemplateId, vals);
                }
            } while (token != null);
        }

        [FunctionName("PublicationSubscribe")]
        public async Task<IActionResult> PublicationSubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]HttpRequest req,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogTrace("PublicationSubscribe function triggered");

            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var subscriptionsTable = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
            var pendingSubscriptionsTable = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
            var client = GetNotifyClient(config);

            string id = req.Query["id"];
            string email = req.Query["email"];
            string slug = req.Query["slug"];
            string title = req.Query["title"];

            bool subscriptionPending = false;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            id = id ?? data?.id;
            email = email ?? data?.email;
            slug = slug ?? data?.slug;
            title = title ?? data?.title;

            logger.LogInformation($"email: {email} id: {id} slug: {slug} title: {title}");

            if (id == null || email == null || slug == null || title == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            try
            {
                subscriptionPending = _storageTableService.RetrieveSubscriber(pendingSubscriptionsTable, new SubscriptionEntity(id, email)).Result != null ? true : false;

                logger.LogInformation($"pending subscription found? : {subscriptionPending}");

                // If already existing and pending then don't send another one
                if (!subscriptionPending)
                {
                    // Now check if already subscribed

                    if (_storageTableService
                            .RetrieveSubscriber(subscriptionsTable, new SubscriptionEntity(id, email)).Result !=
                        null)
                    {
                        return (ActionResult)new BadRequestObjectResult("You are already subscribed to this publication.");
                    }

                    // Verification Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = _tokenService.GenerateToken(tokenSecretKey, email, expiryDateTime);

                    await _storageTableService.UpdateSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email, title, slug, expiryDateTime));

                    var vals = new Dictionary<string, dynamic>
                    {
                        {"publication_name", title},
                        {"verification_link", baseUrl + id + "/verify-subscription/" + activationCode}
                    };

                    _emailService.sendEmail(client, email, emailTemplateId, vals);
                }

                return new OkObjectResult("Thanks! Please check your email.");
            }
            catch (NotifyClientException ex)
            {
                // Remove the subscriber from storage if we could not successfully send the email & just added it
                if (!subscriptionPending)
                {
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTable, new SubscriptionEntity(id, email));
                }

                return new BadRequestObjectResult($"There are problems sending the subscription verification email: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"There are problems storing your subscription: {ex.Message}");
            }
        }

        [FunctionName("PublicationUnsubscribe")]
        public async Task<IActionResult> PublicationUnsubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token)
        {
            logger.LogTrace("PublicationUnsubscribe function triggered");

            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            logger.LogInformation($"Unsubscribe:{ email } from {id}");

            if (email != null)
            {
                var table = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
                var sub = new SubscriptionEntity(id, email);
                sub = _storageTableService.RetrieveSubscriber(table, sub).Result;

                if (sub != null)
                {
                    await _storageTableService.RemoveSubscriber(table, sub);
                    logger.LogInformation(
                        $"redirect to:{webApplicationBaseUrl}subscriptions?slug={sub.Slug}?unsubscribed=true");

                    return new RedirectResult(
                        webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&unsubscribed=true", true);
                }
            }

            return new BadRequestObjectResult("Unable to unsubscribe");
        }

        [FunctionName("PublicationSubscriptionVerify")]
        public async Task<IActionResult> PublicationSubscriptionVerifyFunc([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]HttpRequestMessage req,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token)
        {
            logger.LogTrace("PublicationSubscriptionVerify function triggered");

            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey);

            if (email != null)
            {
                var pendingSubscriptionsTbl = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
                var subscriptionsTbl = GetCloudTable(_storageTableService, config, SubscriptionsTblName);
                var sub = _storageTableService.RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntity(id, email)).Result;

                if (sub != null)
                {
                    // Remove the pending subscription from the the file now verified
                    logger.LogInformation($"removing {email} from pending subscribers");
                    await _storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, sub);
                    sub.DateTimeCreated = DateTime.UtcNow;
                    // Add them to the verified subscribers table
                    logger.LogInformation($"adding {email} to subscribers");
                    await _storageTableService.UpdateSubscriber(subscriptionsTbl, sub);
                    return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&verified=true", true);
                }
            }

            return new BadRequestObjectResult("Unable to verify subscription");
        }

        [FunctionName("RemoveNonVerifiedSubscriptions")]
        public async void RemoveNonVerifiedSubscriptionsFunc([TimerTrigger("0 0 * * * *")]TimerInfo myTimer,
            ExecutionContext context,
            ILogger logger)
        {
            logger.LogTrace($"PublicationSubscriptionVerify function triggered at: {DateTime.Now}");

            var config = LoadAppSettings(context);
            var pendingSubscriptionsTbl = GetCloudTable(_storageTableService, config, PendingSubscriptionsTblName);
            // Remove any pending subscriptions where the token has expired i.e. more than 1 hour old
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("DateTimeCreated", QueryComparisons.LessThan, DateTime.UtcNow.AddHours(1)));

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SubscriptionEntity> resultSegment = await pendingSubscriptionsTbl.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (SubscriptionEntity entity in resultSegment.Results)
                {
                    logger.LogInformation($"Removing {entity.RowKey}");
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

        private static CloudTable GetCloudTable(IStorageTableService storageTableService, IConfiguration config, string tableName)
        {
            var connectionStr = config.GetConnectionString(StorageConnectionName);
            return storageTableService.GetTable(config, connectionStr, tableName).Result;
        }

        private static PublicationNotification ExtractNotification(JObject publicationNotification)
        {
            return publicationNotification.ToObject<PublicationNotification>();
        }
    }
}
