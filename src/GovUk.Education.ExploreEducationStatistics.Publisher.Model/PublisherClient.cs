#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public class PublisherClient(string connectionString) : IPublisherClient
{
    private readonly QueueServiceClient _queueServiceClient = new(connectionString);

    public async Task PublishMethodologyFiles(Guid methodologyId, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(
            PublisherQueues.PublishMethodologyFilesQueue,
            new PublishMethodologyFilesMessage(methodologyId),
            cancellationToken
        );
    }

    public async Task PublishReleaseFiles(
        IReadOnlyList<ReleasePublishingKey> releasePublishingKeys,
        CancellationToken cancellationToken = default
    )
    {
        await _queueServiceClient.SendMessageAsJson(
            PublisherQueues.PublishReleaseFilesQueue,
            new PublishReleaseFilesMessage(releasePublishingKeys),
            cancellationToken
        );
    }

    public async Task PublishTaxonomy(CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(
            PublisherQueues.PublishTaxonomyQueue,
            new PublishTaxonomyMessage(),
            cancellationToken
        );
    }

    public async Task HandleReleaseChanged(
        ReleasePublishingKey releasePublishingKey,
        bool immediate,
        CancellationToken cancellationToken = default
    )
    {
        await _queueServiceClient.SendMessageAsJson(
            PublisherQueues.NotifyChangeQueue,
            new NotifyChangeMessage(immediate, releasePublishingKey),
            cancellationToken
        );
    }
}
