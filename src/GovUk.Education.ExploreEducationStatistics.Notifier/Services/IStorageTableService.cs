using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface IStorageTableService
    {
        Task UpdateSubscriber(CloudTable table, SubscriptionEntity subscription);

        Task RemoveSubscriber(CloudTable table, SubscriptionEntity subscription);

        Task<SubscriptionEntity> RetrieveSubscriber(CloudTable table, SubscriptionEntity subscription);

        Task<CloudTable> GetTable(string connectionStr, string storageTableName);
    }
}