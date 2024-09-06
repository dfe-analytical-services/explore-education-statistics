using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;

public class PublicationSubscriptionRepository(IOptions<AppSettingsOptions> appSettingsOptions) : IPublicationSubscriptionRepository
{
    private readonly AppSettingsOptions _appSettingsOptions = appSettingsOptions.Value;

    public async Task UpdateSubscriber(CloudTable table, SubscriptionEntityOld subscription)
    {
        await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
    }

    public async Task RemoveSubscriber(CloudTable table, SubscriptionEntityOld subscription)
    {
        subscription.ETag = "*"; // @MarkFix Do I need to do this? Looks like it
        await table.ExecuteAsync(TableOperation.Delete(subscription));
    }

    public async Task<SubscriptionEntityOld?> RetrieveSubscriber(CloudTable table, SubscriptionEntityOld subscription)
    {
        // Need to define the extra columns to retrieve
        var columns = new List<string>
        {
            "Verified",
            "Slug",
            "Title"
        };
        var result = await table.ExecuteAsync(
            TableOperation.Retrieve<SubscriptionEntityOld>(subscription.PartitionKey, subscription.RowKey, columns));
        return (SubscriptionEntityOld)result.Result;
    }

    public async Task<CloudTable> GetTable(string storageTableName)
    {
        var storageAccount = CloudStorageAccount.Parse(_appSettingsOptions.NotifierStorageConnectionString);
        var tableClient = storageAccount.CreateCloudTableClient();
        var table = tableClient.GetTableReference(storageTableName);
        await table.CreateIfNotExistsAsync();
        return table;
    }

    public async Task<SubscriptionOld> GetSubscription(string id, string email) // @MarkFix remove
    {
        var pendingSub =
            await RetrieveSubscriber(await GetTable(NotifierTableStorage.PublicationPendingSubscriptionsTable),
                new SubscriptionEntityOld(id, email));
        if (pendingSub is not null)
        {
            return new SubscriptionOld
            {
                Subscriber = pendingSub,
                Status = SubscriptionStatus.SubscriptionPending
            };
        }

        var activeSubscriber = await RetrieveSubscriber(await GetTable(NotifierTableStorage.PublicationSubscriptionsTable),
            new SubscriptionEntityOld(id, email));
        if (activeSubscriber is not null)
        {
            return new SubscriptionOld
            {
                Subscriber = activeSubscriber,
                Status = SubscriptionStatus.Subscribed
            };
        }

        return new SubscriptionOld { Status = SubscriptionStatus.NotSubscribed };
    }
}
