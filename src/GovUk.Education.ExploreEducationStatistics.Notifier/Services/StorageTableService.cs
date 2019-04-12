using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class StorageTableService : IStorageTableService
    {
        public async Task UpdateSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            // N.B. InsertOrMerge doesn't work if using the Azurite container on Linux so for testing
            // can change InsertOrMerge to InsertOrReplace (but will only work for subscribing not updates or verifications 
            TableResult tableResult = await table.ExecuteAsync(TableOperation.InsertOrMerge(subscription));
        }
        
        public async Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            await table.ExecuteAsync(TableOperation.Delete(subscription));
        }
        
        public async Task<SubscriptionEntity> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            var result = await table.ExecuteAsync(TableOperation.Retrieve(subscription.PartitionKey, subscription.RowKey));
            return (SubscriptionEntity)result.Result;
        }

        public async Task<CloudTable> GetTable(IConfiguration config, string connectionStr, string storageTableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionStr);                
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();                
            CloudTable table = tableClient.GetTableReference(storageTableName);
            table.CreateIfNotExistsAsync();
            return table;
        }
    }
}