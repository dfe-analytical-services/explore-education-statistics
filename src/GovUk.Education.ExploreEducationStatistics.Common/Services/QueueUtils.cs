using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class QueueUtils
    {
        public static CloudQueue GetQueueReference(string storageConnectionString, string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            return queue;
        }

        public static async Task<CloudQueue> GetQueueReferenceAsync(string storageConnectionString, string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }
    }
}