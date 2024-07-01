#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IQueueServiceClient
{
    Task<QueueClient> GetQueueClient(string queueName, CancellationToken cancellationToken = default);

    public Task SendMessagesAsJson<T>(
        string queueName, IReadOnlyList<T> messages, CancellationToken cancellationToken = default);

    public Task SendMessageAsJson<T>(string queueName, T message, CancellationToken cancellationToken = default);

    public Task SendMessage(string queueName, string message, CancellationToken cancellationToken = default);
}
