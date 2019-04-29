using System.Collections.Generic;
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
            TableResult tableResult = await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
        }
        
        public async Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            subscription.ETag = "*";
            await table.ExecuteAsync(TableOperation.Delete(subscription));
        }
        
        public async Task<SubscriptionEntity> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {   
            // Need to define the extra columns to retrieve
            var columns = new List<string>(){ "Verified", "Slug", "Title" };
            var result = await table.ExecuteAsync(TableOperation.Retrieve<SubscriptionEntity>(subscription.PartitionKey, subscription.RowKey, columns));
            return (SubscriptionEntity) result.Result;
        }

        public async Task<CloudTable> GetTable(IConfiguration config, string connectionStr, string storageTableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionStr);                
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();                
            CloudTable table = tableClient.GetTableReference(storageTableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}