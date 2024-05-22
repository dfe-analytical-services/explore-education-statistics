using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class StorageTableService(IOptions<AppSettingOptions> appSettingOptions) : IStorageTableService
    {
        private readonly AppSettingOptions _appSettingOptions = appSettingOptions.Value;

        public async Task UpdateSubscriber(CloudTable table, SubscriptionEntity subscription)
        {
            await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
        }

        public async Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {
            subscription.ETag = "*";
            await table.ExecuteAsync(TableOperation.Delete(subscription));
        }

        public async Task<SubscriptionEntity?> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription)
        {
            // Need to define the extra columns to retrieve
            var columns = new List<string>()
            {
                "Verified", "Slug", "Title"
            };
            var result = await table.ExecuteAsync(
                TableOperation.Retrieve<SubscriptionEntity>(subscription.PartitionKey, subscription.RowKey, columns));
            return (SubscriptionEntity)result.Result;
        }

        public async Task<CloudTable> GetTable(string storageTableName)
        {
            var storageAccount = CloudStorageAccount.Parse(_appSettingOptions.TableStorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(storageTableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<Subscription> GetSubscription(string id, string email)
        {
            var pendingSub =
                await RetrieveSubscriber(await GetTable(NotifierPendingSubscriptionsTableName),
                        new SubscriptionEntity(id, email));
            if (pendingSub is not null)
            {
                return new Subscription() { Subscriber = pendingSub, Status = SubscriptionStatus.SubscriptionPending };
            }
            
            var activeSubscriber = await RetrieveSubscriber(await GetTable(NotifierSubscriptionsTableName), new SubscriptionEntity(id, email));
            if (activeSubscriber is not null)
            {
                return new Subscription() { Subscriber = activeSubscriber, Status = SubscriptionStatus.Subscribed };
            }
            
            return new Subscription() { Status = SubscriptionStatus.NotSubscribed };
        }
    }
}
