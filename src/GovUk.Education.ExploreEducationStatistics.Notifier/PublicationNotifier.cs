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
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

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

        [FunctionName("PublicationNotifier")]
        public static void PublicationNotifierFunc(
            [QueueTrigger("publication-queue", Connection = "")]
            JObject pn,
            [Inject]ITokenService tokenService,
            [Inject]IEmailService emailService,
            [Inject]IStorageTableService storageTableService,
            ILogger logger,
            ExecutionContext context)
        {            
            
            logger.LogInformation($"C# Queue trigger function processed: {pn.ToString()}");
            
            var publicationNotification = extractNotification(pn);           
            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(NotificationEmailTemplateIdName);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var client = GetNotifyClient(config);
            var table = GetCloudTable(storageTableService, config, SubscriptionsTblName);            
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
                    var unsubscribeToken = tokenService.GenerateToken(tokenSecretKey, entity.RowKey, DateTime.UtcNow.AddYears(1));
                    var vals = new Dictionary<string, dynamic>
                       {
                           // Use values from the queue just in case the name or slug of the publication chnages
                           {"publication_name", publicationNotification.Name},
                           {"publication_link", webApplicationBaseUrl + "statistics/" + publicationNotification.Slug},
                           {"unsubscribe_link", baseUrl + entity.PartitionKey + "/unsubscribe/" + unsubscribeToken}
                       };
                   
                    emailService.sendEmail(client, entity.RowKey, emailTemplateId, vals); 
                }
            } while (token != null);
        }
        
        [FunctionName("PublicationSubscribe")]
        public static async Task<IActionResult> PublicationSubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]HttpRequest req,
            [Inject]ITokenService tokenService,
            [Inject]IEmailService emailService,
            [Inject]IStorageTableService storageTableService,
            ILogger logger,
            ExecutionContext context)
        {            
            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var connectionStr = config.GetConnectionString(StorageConnectionName);
            var subscriptionsTable = GetCloudTable(storageTableService, config, SubscriptionsTblName);
            var pendingSubscriptionsTable = GetCloudTable(storageTableService, config, PendingSubscriptionsTblName);
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
                subscriptionPending = storageTableService.RetrieveSubscriber(pendingSubscriptionsTable, new SubscriptionEntity(id, email)).Result != null ? true : false;

                logger.LogInformation($"pending subscription found? : {subscriptionPending}");

                // If already existing and pending then don't send another one
                if (!subscriptionPending)
                {
                    // Now check if already subscribed

                    if (storageTableService
                            .RetrieveSubscriber(subscriptionsTable, new SubscriptionEntity(id, email)).Result !=
                        null)
                    {
                        return (ActionResult)new BadRequestObjectResult("You are already subscribed to this publication.");
                    }

                    // Verifciation Token expires in 1 hour
                    var expiryDateTime = DateTime.UtcNow.AddHours(1);
                    var activationCode = tokenService.GenerateToken(tokenSecretKey, email, expiryDateTime);

                    await storageTableService.UpdateSubscriber(pendingSubscriptionsTable,
                        new SubscriptionEntity(id, email, title, slug, expiryDateTime));

                    var vals = new Dictionary<string, dynamic>
                    {
                        {"publication_name", title},
                        {"verification_link", baseUrl + id + "/verify-subscription/" + activationCode}
                    };

                    emailService.sendEmail(client, email, emailTemplateId, vals);
                }

                return new OkObjectResult("Thanks! Please check your email.");
            }
            catch (NotifyClientException ex)
            {
                // Remove the subscriber from storage if we could not successfully send the email & just added it
                if (!subscriptionPending)
                {
                    await storageTableService.RemoveSubscriber(pendingSubscriptionsTable, new SubscriptionEntity(id, email));
                }

                return new BadRequestObjectResult($"There are problems sending the subscription verification email: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"There are problems storing your subscription: {ex.Message}");
            }
        }
        
        [FunctionName("PublicationUnsubscribe")]
        public static async Task<IActionResult> PublicationUnsubscribeFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]HttpRequestMessage req,
            [Inject]ITokenService tokenService,
            [Inject]IStorageTableService storageTableService,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var email = tokenService.GetEmailFromToken(token, tokenSecretKey);

            logger.LogInformation($"Unsubscribe:{ email } from {id}");
            
            if (email != null)
            {
                var table = GetCloudTable(storageTableService, config, SubscriptionsTblName);
                var sub = new SubscriptionEntity(id, email);
                sub = storageTableService.RetrieveSubscriber(table, sub).Result;

                if (sub != null)
                {
                    await storageTableService.RemoveSubscriber(table, sub);
                    logger.LogInformation(
                        $"redirect to:{webApplicationBaseUrl}subscriptions?slug={sub.Slug}?unsubscribed=true");

                    return new RedirectResult(
                        webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&unsubscribed=true", true);
                }
            }
           
            return new BadRequestObjectResult("Unable to unsubscribe");
        }
       
        [FunctionName("PublicationSubscriptionVerify")]
        public static async Task<IActionResult> PublicationSubscriptionVerifyFunc([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]HttpRequestMessage req,
            [Inject]ITokenService tokenService,
            [Inject]IStorageTableService storageTableService,
            ILogger logger,
            ExecutionContext context,
            string id,
            string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var email = tokenService.GetEmailFromToken(token, tokenSecretKey);
            
            if (email != null)
            {
                var pendingSubscriptionsTbl = GetCloudTable(storageTableService, config, PendingSubscriptionsTblName);
                var subscriptionsTbl = GetCloudTable(storageTableService, config, SubscriptionsTblName);
                var sub = storageTableService.RetrieveSubscriber(pendingSubscriptionsTbl, new SubscriptionEntity(id, email)).Result;
                
                if (sub != null)
                {
                    // Remove the pending subscription from the the file now verified
                    logger.LogInformation($"removing {email} from pending subscribers");
                    await storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, sub);
                    sub.DateTimeCreated = DateTime.UtcNow;
                    // Add them to the verified subscribers table
                    logger.LogInformation($"adding {email} to subscribers");
                    await storageTableService.UpdateSubscriber(subscriptionsTbl, sub);
                    return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&verified=true", true);
                }
            }  
           
            return new BadRequestObjectResult("Unable to verify subscription");
        }
        
        [FunctionName("RemoveNonVerifiedSubcriptions")]
        public static async void RemoveNonVerifiedSubcriptionsFunc([TimerTrigger("0 0 * * * *")]TimerInfo myTimer,
            [Inject]IStorageTableService storageTableService,
            ExecutionContext context,
            ILogger logger)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = LoadAppSettings(context);
            var pendingSubscriptionsTbl = GetCloudTable(storageTableService, config, PendingSubscriptionsTblName);
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
                    await storageTableService.RemoveSubscriber(pendingSubscriptionsTbl, entity);
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
        
        private static PublicationNotification extractNotification(JObject publicationNotification)
        {
            return publicationNotification.ToObject<PublicationNotification>();          
        }
    }
}
