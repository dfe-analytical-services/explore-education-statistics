using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;

public interface IPublicationSubscriptionRepository
{
    Task UpdateSubscriber(CloudTable table, SubscriptionEntityOld subscription);

    Task RemoveSubscriber(CloudTable table, SubscriptionEntityOld subscription);

    Task<SubscriptionEntityOld?> RetrieveSubscriber(CloudTable table, SubscriptionEntityOld subscription);

    Task<CloudTable> GetTable(string storageTableName);

    Task<SubscriptionOld> GetSubscription(string id, string email);
}
