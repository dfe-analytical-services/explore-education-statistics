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

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class PublicationNotifier
    {                   
        private const string StorageTableName = "Subscriptions";
        private const string StorageConnectionName = "TableStorageConnString";
        private const string NotifyApiKeyName = "NotifyApiKey";
        private const string BaseUrlName = "BaseUrl";
        private const string WebApplicationBaseUrlName = "WebApplicationBaseUrl";
        private const string TokenSecretKeyName = "TokenSecretKey";
        private const string NotificationEmailTemplateIdName = "PublicationNotificationEmailTemplateId";
        private const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";

        // Dependency injection is not quite there yet in Azure functions so create the services.
        private readonly IEmailService _emailService = new EmailService();
        private readonly ITokenService _tokenService = new TokenService();
        private readonly IStorageTableService _storageTableService = new StorageTableService();

        [FunctionName("PublicationNotifier")]
        public async void Run1([QueueTrigger("publication-queue", Connection = "")]
            JObject pn, ILogger log, ExecutionContext context)
        {            
            
            log.LogInformation($"C# Queue trigger function processed: {pn.ToString()}");
            
            var publicationNotification = extractNotification(pn);
            
            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(NotificationEmailTemplateIdName);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

            var client = GetNotifyClient(config);
            var table = GetCloudTable(config);

            log.LogInformation($"C# ServiceBus queue trigger function processed message with pub id: {publicationNotification.PublicationId}");
            
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, publicationNotification.PublicationId));
            
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SubscriptionEntity> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (SubscriptionEntity entity in resultSegment.Results)
                {
                    log.LogInformation($"There are {resultSegment.Results.Count} subscribed to this publication");
                    if (entity.Verified)
                    {
                       var unsubscribeToken = _tokenService.GenerateToken(tokenSecretKey, entity.RowKey, log);
                       var vals = new Dictionary<string, dynamic>
                           {
                               // Use values from the queue just in case the name or slug of the publication chnages
                               {"publication_name", publicationNotification.Name},
                               {"publication_link", webApplicationBaseUrl + "statistics/" + publicationNotification.Slug},
                               {"unsubscribe_link", baseUrl + entity.PartitionKey + "/unsubscribe/" + unsubscribeToken}
                           };
                       
                       _emailService.sendEmail(client, entity.RowKey, emailTemplateId, vals); 
                    }
                }
            } while (token != null);
            
            log.LogInformation("done");
        }
        
        [FunctionName("PublicationSubscribe")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publication/subscribe/")]HttpRequest req, 
            ILogger log, ExecutionContext context)
        {            
            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var connectionStr = config.GetConnectionString(StorageConnectionName);

            var client = GetNotifyClient(config);

            string id = req.Query["id"];
            string email = req.Query["email"];
            string slug = req.Query["slug"];
            string title = req.Query["title"];
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
         
            id = id ?? data?.id;
            email = email ?? data?.email;
            slug = slug ?? data?.slug;
            title = title ?? data?.title;
            
            log.LogInformation($"email: {email} id: {id} slug: {slug} title: {title}");

            if (id == null || email == null || slug == null || title == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            try
            {
                var table = _storageTableService.GetTable(config, connectionStr, StorageTableName).Result;
                var activationCode = _tokenService.GenerateToken(tokenSecretKey, email, log);
                
                await _storageTableService.UpdateSubscriber(table, new SubscriptionEntity(id, email, title, slug));
                var vals = new Dictionary<string, dynamic>
                {
                    {"publication_name", title},
                    {"verification_link", baseUrl + id + "/verify-subscription/" + activationCode}
                };
                                
                _emailService.sendEmail(client, email, emailTemplateId, vals);
                return (ActionResult) new OkObjectResult("Thanks! Please check your email.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"There are problems storing your subscription: {ex.GetType()}");
            }
        }
        
        [FunctionName("PublicationUnsubscribe")]
        public async Task<IActionResult> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/unsubscribe/{token}")]
            HttpRequestMessage req, ILogger log, ExecutionContext context, string id, string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);

            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey, log);

            if (email != null)
            {
                var table = GetCloudTable(config);

                var sub = new SubscriptionEntity(id, email, null, null);
                await _storageTableService.RemoveSubscriber(table, sub);
                log.LogInformation($"redirect to:{ webApplicationBaseUrl }subscriptions?slug={sub.Slug}?unsubscribed=true");

                return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&unsubscribed=true", true);
            }
           
            return new BadRequestObjectResult("Unable to unsubscribe");
        }
       
        [FunctionName("PublicationSubscriptionVerify")]
        public async Task<IActionResult> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publication/{id}/verify-subscription/{token}")]
            HttpRequestMessage req, ILogger log, ExecutionContext context, string id, string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var webApplicationBaseUrl = config.GetValue<string>(WebApplicationBaseUrlName);

            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey, log);
            
            if (email != null)
            {
                var table = GetCloudTable(config);
                var sub = new SubscriptionEntity(id, email, null, null);
                sub = _storageTableService.RetrieveSubscriber(table, sub).Result;
                sub.Verified = true;
                await _storageTableService.UpdateSubscriber(table, sub);
                log.LogInformation($"redirect to:{ webApplicationBaseUrl }subscriptions?slug={sub.Slug}?verified=true");

                return new RedirectResult(webApplicationBaseUrl + $"subscriptions?slug={sub.Slug}&verified=true", true);
            }  
           
            return new BadRequestObjectResult("Unable to verify subscription");
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

        private CloudTable GetCloudTable(IConfiguration config)
        {
            var connectionStr = config.GetConnectionString(StorageConnectionName);
            return _storageTableService.GetTable(config, connectionStr, StorageTableName).Result;
        }
        
        private PublicationNotification extractNotification(JObject publicationNotification)
        {
            return publicationNotification.ToObject<PublicationNotification>();          
        }
    }
}
