using System.Text.Json;
using Azure.Storage.Queues;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class QueueServiceClient(string connectionString) : IQueueServiceClient
{
    private readonly Azure.Storage.Queues.QueueServiceClient _queueServiceClient = new(
        connectionString,
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 }
    );

    public async Task SendMessagesAsJson<T>(
        string queueName,
        IReadOnlyList<T> messages,
        CancellationToken cancellationToken = default
    )
    {
        var queueClient = await GetQueueClient(queueName, cancellationToken);
        await messages
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async message =>
                {
                    var messageAsJson = JsonSerializer.Serialize(message);
                    await queueClient.SendMessageAsync(messageAsJson, cancellationToken);
                },
                cancellationToken: cancellationToken
            );
    }

    public async Task SendMessageAsJson<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var messageAsJson = JsonSerializer.Serialize(message);
        await SendMessage(queueName, messageAsJson, cancellationToken);
    }

    public async Task SendMessage(string queueName, string message, CancellationToken cancellationToken = default)
    {
        var queueClient = await GetQueueClient(queueName, cancellationToken);
        await queueClient.SendMessageAsync(message, cancellationToken);
    }

    private async Task<QueueClient> GetQueueClient(string queueName, CancellationToken cancellationToken = default)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return queueClient;
    }
}
