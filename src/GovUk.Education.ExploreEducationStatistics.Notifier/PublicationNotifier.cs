using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Notify.Client;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class PublicationNotifier
    {                   
        private const string StorageTableName = "Subscriptions";
        private const string StorageConnectionName = "TableStorageConnString";
        private const string NotifyApiKeyName = "NotifyApiKey";
        private const string BaseUrlName = "BaseUrl";
        private const string TokenSecretKeyName = "TokenSecretKey";
        private const string NotificationEmailTemplateIdName = "PublicationNotificationEmailTemplateId";
        private const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";

        // Dependency injection is not quite there yet in Azure functions so create the services.
        private readonly IEmailService _emailService = new EmailService();
        private readonly ITokenService _tokenService = new TokenService();

        [FunctionName("PublicationNotifier")]
        public async void Run1([QueueTrigger("publication-queue", Connection = "")]
            string publicationId, ILogger log, ExecutionContext context)
        {            
            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(NotificationEmailTemplateIdName);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var client = GetNotifyClient(config);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var partitionKey = publicationId;
            var table = GetTable(config).Result;
            var query = new TableQuery<SubscriptionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<SubscriptionEntity> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (SubscriptionEntity entity in resultSegment.Results)
                {
                    if (entity.Verified)
                    {
                       var unsubscribeToken = _tokenService.GenerateToken(tokenSecretKey, entity.RowKey, log);
                       var vals = new Dictionary<string, dynamic>
                           {
                               {"publication_name", "My Publication"},
                               {"publication_link", "https://somelink"},
                               {"unsubscribe_link", baseUrl + publicationId + "/unsubscribe/" + unsubscribeToken}
                           };
                       
                       _emailService.sendEmail(client, entity.RowKey, emailTemplateId, vals); 
                    }
                }
            } while (token != null);
            
            log.LogInformation("done");
        }
        
        [FunctionName("PublicationSubscribe")]
        public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Function, "post", Route = "publication/subscribe/")]HttpRequest req, 
            ILogger log, ExecutionContext context)
        {            
            string email = req.Query["email"];
            string publicationId = req.Query["publicationId"];
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
         
            email = email ?? data?.email;
            publicationId = publicationId ?? data?.publicationId;
            
            var config = LoadAppSettings(context);
            var baseUrl = config.GetValue<string>(BaseUrlName);
            var client = GetNotifyClient(config);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);

            log.LogInformation($"email {email} publicationId {publicationId}");

            if (email == null || publicationId == null)
            {
                return new BadRequestObjectResult("Please pass a valid email & publication");
            }

            try
            {
                var table = GetTable(config).Result;
                var activationCode = _tokenService.GenerateToken(tokenSecretKey, email, log);
                
                await AddSubscriber(table, new SubscriptionEntity(publicationId, email));
                var vals = new Dictionary<string, dynamic>
                {
                    {"publication_name", "My Publication"},
                    {"verification_link", baseUrl + publicationId + "/verify-subscription/" + activationCode}
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
        public async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Function, "get", Route = "publication/{publicationId}/unsubscribe/{token}")]
            HttpRequestMessage req, ILogger log, ExecutionContext context, string publicationId, string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey, log);
            
            if (email != null)
            {
                var table = GetTable(config).Result;
                var sub = new SubscriptionEntity(publicationId, email);
                await RemoveSubscriber(table, sub);
                return req.CreateResponse(HttpStatusCode.OK, "Thanks! Unsubscribed.");
            }
           
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
       
        [FunctionName("PublicationSubscriptionVerify")]
        public async Task<HttpResponseMessage> Run4([HttpTrigger(AuthorizationLevel.Function, "get", Route = "publication/{publicationId}/verify-subscription/{token}")]
            HttpRequestMessage req, ILogger log, ExecutionContext context, string publicationId, string token)
        {
            var config = LoadAppSettings(context);
            var tokenSecretKey = config.GetValue<string>(TokenSecretKeyName);
            var email = _tokenService.GetEmailFromToken(token, tokenSecretKey, log);
            
            if (email != null)
            {
                var table = GetTable(config).Result;
                var sub = new SubscriptionEntity(publicationId, email);
                sub.Verified = true;
                await AddSubscriber(table, sub);
                return req.CreateResponse(HttpStatusCode.OK, "Thanks! Verified.");
            }
           
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        
        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
        
        private static async Task AddSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
        }
        
        private static async Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            await table.ExecuteAsync(TableOperation.Delete(subscription));
        }

        private static async Task<CloudTable> GetTable(IConfiguration config)
        {
            var connectionStr = config.GetConnectionString(StorageConnectionName);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionStr);                
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();                
            CloudTable table = tableClient.GetTableReference(StorageTableName);
            table.CreateIfNotExistsAsync();
            return table;
        }

        private static NotificationClient GetNotifyClient(IConfiguration config)
        {
            var notifyApiKey = config.GetValue<string>(NotifyApiKeyName);
            return new NotificationClient(notifyApiKey);
        }
    }
}
