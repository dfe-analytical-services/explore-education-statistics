#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IQueueServiceClient
{
    public Task SendMessagesAsJson<T>(
        string queueName,
        IReadOnlyList<T> messages,
        CancellationToken cancellationToken = default
    );

    public Task SendMessageAsJson<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken = default
    );

    public Task SendMessage(
        string queueName,
        string message,
        CancellationToken cancellationToken = default
    );
}
