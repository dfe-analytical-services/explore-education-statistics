#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public class PublisherClient(string connectionString) : IPublisherClient
{
    private readonly QueueServiceClient _queueServiceClient = new(connectionString);

    public async Task PublishMethodologyFiles(Guid methodologyId, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.PublishMethodologyFilesQueue,
            new PublishMethodologyFilesMessage(methodologyId),
            cancellationToken);
    }

    public async Task PublishReleaseContent(
        ReleasePublishingKeyOld releasePublishingKeyOld, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.PublishReleaseContentQueue,
            new PublishReleaseContentMessage(releasePublishingKeyOld),
            cancellationToken);
    }

    public async Task PublishReleaseFiles(
        IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.PublishReleaseFilesQueue,
            new PublishReleaseFilesMessage(releasePublishingKeys),
            cancellationToken);
    }

    public async Task PublishTaxonomy(CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.PublishTaxonomyQueue,
            new PublishTaxonomyMessage(),
            cancellationToken);
    }

    public async Task HandleReleaseChanged(
        ReleasePublishingKeyOld releasePublishingKeyOld, bool immediate, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.NotifyChangeQueue,
            new NotifyChangeMessage(immediate, releasePublishingKeyOld),
            cancellationToken);
    }

    public async Task RetryReleasePublishing(Guid releaseVersionId, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.RetryReleasePublishingQueue,
            new RetryReleasePublishingMessage(releaseVersionId),
            cancellationToken);
    }

    public async Task StageReleaseContent(
        IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(PublisherQueues.StageReleaseContentQueue,
            new StageReleaseContentMessage(releasePublishingKeys),
            cancellationToken);
    }
}
