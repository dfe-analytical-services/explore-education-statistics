#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class StorageQueueService : IStorageQueueService
    {
        private readonly string _storageConnectionString;
        private readonly IStorageInstanceCreationUtil _storageInstanceCreationUtil;

        public StorageQueueService(
            string storageConnectionString,
            IStorageInstanceCreationUtil storageInstanceCreationUtil)
        {
            _storageConnectionString = storageConnectionString;
            _storageInstanceCreationUtil = storageInstanceCreationUtil;
        }

        public void AddMessage(string queueName, object value)
        {
            var queue = GetQueueReference(queueName);
            queue.AddMessage(ToCloudQueueMessage(value));
        }

        public async Task AddMessageAsync(string queueName, object value)
        {
            var queue = await GetQueueReferenceAsync(queueName);
            await queue.AddMessageAsync(ToCloudQueueMessage(value));
        }

        public async Task AddMessages<T>(string queueName, List<T> values)
        {
            var queue = await GetQueueReferenceAsync(queueName);
            await values
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async value => { await queue.AddMessageAsync(ToCloudQueueMessage(value)); });
        }

        public async Task<int?> GetApproximateMessageCount(string queueName)
        {
            var queue = await GetQueueReferenceAsync(queueName);
            await queue.FetchAttributesAsync();
            return queue.ApproximateMessageCount;
        }

        public async Task Clear(string queueName)
        {
            var queue = await GetQueueReferenceAsync(queueName);
            await queue.ClearAsync();
        }

        private CloudQueue GetQueueReference(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);

            _storageInstanceCreationUtil.CreateInstanceIfNotExists(
                _storageConnectionString,
                AzureStorageType.Queue,
                queueName,
                () => queue.CreateIfNotExists());

            return queue;
        }

        private async Task<CloudQueue> GetQueueReferenceAsync(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);

            await _storageInstanceCreationUtil.CreateInstanceIfNotExistsAsync(
                _storageConnectionString,
                AzureStorageType.Queue,
                queueName,
                () => queue.CreateIfNotExistsAsync());

            return queue;
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}
