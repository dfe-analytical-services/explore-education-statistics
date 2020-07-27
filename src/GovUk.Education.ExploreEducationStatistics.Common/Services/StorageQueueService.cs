﻿using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class StorageQueueService : IStorageQueueService
    {
        private readonly string _storageConnectionString;

        public StorageQueueService(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public void AddMessages(string queueName, params object[] values)
        {
            var queue = GetQueueReference(queueName);
            foreach (var value in values)
            {
                queue.AddMessage(ToCloudQueueMessage(value));
            }
        }

        public async Task AddMessagesAsync(string queueName, params object[] values)
        {
            var queue = await GetQueueReferenceAsync(queueName);
            foreach (var value in values)
            {
                await queue.AddMessageAsync(ToCloudQueueMessage(value));
            }
        }

        private CloudQueue GetQueueReference(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            return queue;
        }

        private async Task<CloudQueue> GetQueueReferenceAsync(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        private static CloudQueueMessage ToCloudQueueMessage(object value)
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(value));
        }
    }
}