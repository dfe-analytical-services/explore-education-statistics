using System;
using System.Collections.Generic;
using System.Linq;
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
using Notify.Models.Responses;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public static class PublicationNotifier
    {                   
        const string StorageTableName = "Subscriptions";
        const string StorageConnectionName = "TableStorageConnString";
        const string NotifyApiKeyName = "NotifyApiKey";
        const string NotificationEmailTemplateIdName = "PublicationNotificationEmailTemplateId";
        const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";
        
        [FunctionName("PublicationNotifier")]
        public static async void Run1([QueueTrigger("publication-queue", Connection = "")]
            string publicationId, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"publicationId: {publicationId}");
            
            var config = LoadAppSettings(context);
            var emailTemplateId = config.GetValue<string>(NotificationEmailTemplateIdName);
            var retList = new List<string>();
            var client = GetNotifyClient(config);
            var partitionKey = "publication-" + publicationId;
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
                       retList.Add(entity.RowKey);
                       var vals = new Dictionary<string, dynamic>
                           {
                               {"publication_name", "My Publication"},
                               {"publication_link", "https://somelink"}
                           };
                       sendEmail(client, entity.RowKey, emailTemplateId, vals); 
                    }
                }
            } while (token != null);
            
            log.LogInformation("done");
        }
        
        [FunctionName("PublicationSubscribe")]
        public static async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Function, "post", Route = "publication/subscribe/")]HttpRequestMessage req, 
            ILogger log, ExecutionContext context)
        {
            var postData = await req.Content.ReadAsFormDataAsync();
            var missingFields = new List<string>();
            var config = LoadAppSettings(context);
            var client = GetNotifyClient(config);
            var emailTemplateId = config.GetValue<string>(VerificationEmailTemplateIdName);

            if (postData["email"] == null)
            {
                missingFields.Add("email");
            }
            if (postData["publication-id"] == null)
            {
                missingFields.Add("publication-id");
            }
            if (missingFields.Any())
            {
                var missingFieldsSummary = String.Join(", ", missingFields);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Missing field(s): {missingFieldsSummary}");
            }

            var email = postData["email"];
            var uri = req.RequestUri.ToString().Replace("/subscribe","");
           
            try
            {
                var table = GetTable(config).Result;
                var activationCode = Guid.NewGuid(); 
                
                await AddSubscriber(table, new SubscriptionEntity(postData["publication-id"], email));
                var vals = new Dictionary<string, dynamic>
                {
                    {"publication_name", "My Publication"},
                    {"verification_link", uri + "verify-subscription/" + activationCode}
                };
                                
                sendEmail(client, email, emailTemplateId, vals);
                return req.CreateResponse(HttpStatusCode.OK, "Thanks! Please check your email."); 
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                
                return req.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    message = $"There are problems storing your subscription: {ex.GetType()}"
                });
            }
        }
       
        [FunctionName("PublicationSubscriptionVerify")]
        public static async Task<HttpResponseMessage> Run3([HttpTrigger(AuthorizationLevel.Function, "get", Route = "publication/verify-subscription/{token}")]
            HttpRequestMessage req, ILogger log, ExecutionContext context, string token)
        {

            log.LogInformation($"Trying to verify using token: {token}");
                
            try
            {
                var config = LoadAppSettings(context);
                var table = GetTable(config).Result;
                var sub = new SubscriptionEntity("1234", "si.shakes@gmail.com");
                
                sub.Verified = true;
                await AddSubscriber(table, sub);

                return req.CreateResponse(HttpStatusCode.OK, "Thanks! Verified."); 
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                
                return req.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    message = $"There are problems verifying subscription: {ex.GetType()}"
                });
            }
        }
        
        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void sendEmail(NotificationClient client, string email, string templateId, Dictionary<string, dynamic> values)
        {
            EmailNotificationResponse response = client.SendEmail(
                emailAddress: email,
                templateId: templateId,
                personalisation: values
                );
        }
        
        private static async Task AddSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
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
