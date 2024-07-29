using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface IPublicationSubscriptionRepository
{
    Task UpdateSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task<SubscriptionEntity?> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription);

    Task<CloudTable> GetTable(string storageTableName);

    Task<Subscription> GetSubscription(string id, string email);
}
