using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface IStorageTableService
{
    Task UpdateSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task<SubscriptionEntity?> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task<CloudTable> GetTable(string storageTableName);
}