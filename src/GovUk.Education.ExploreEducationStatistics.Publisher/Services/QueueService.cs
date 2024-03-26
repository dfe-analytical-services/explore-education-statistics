using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class QueueService : IQueueService
{
    private readonly IStorageQueueService _storageQueueService;
    private readonly IReleasePublishingStatusService _releasePublishingStatusService;
    private readonly ILogger<QueueService> _logger;

    public QueueService(IStorageQueueService storageQueueService,
        IReleasePublishingStatusService releasePublishingStatusService,
        ILogger<QueueService> logger)
    {
        _storageQueueService = storageQueueService;
        _releasePublishingStatusService = releasePublishingStatusService;
        _logger = logger;
    }

    public async Task QueueGenerateStagedReleaseContentMessage(
        IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases)
    {
        var releasesList = releases.ToList();
        _logger.LogInformation(
            "Queuing generate content message for release versions: {ReleaseVersionIds}",
            string.Join(", ", releasesList.Select(tuple => tuple.ReleaseVersionId)));
        await _storageQueueService.AddMessageAsync(
            StageReleaseContentQueue, new StageReleaseContentMessage(releasesList));
        foreach (var (releaseVersionId, releaseStatusId) in releasesList)
        {
            await _releasePublishingStatusService.UpdateContentStageAsync(releaseVersionId: releaseVersionId,
                releaseStatusId: releaseStatusId,
                ReleasePublishingStatusContentStage.Queued);
        }
    }

    public async Task QueuePublishReleaseContentMessage(Guid releaseVersionId, Guid releaseStatusId)
    {
        await QueuePublishReleaseContentMessage(new[] { (releaseVersionId, releaseStatusId) });
    }

    private async Task QueuePublishReleaseContentMessage(
        IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases)
    {
        await releases
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async ids =>
            {
                var (releaseVersionId, releaseStatusId) = ids;
                _logger.LogInformation("Queuing publish content message for release version: {ReleaseVersionId}",
                    releaseVersionId);
                await _storageQueueService.AddMessageAsync(
                    PublishReleaseContentQueue,
                    new PublishReleaseContentMessage(releaseVersionId, releaseStatusId));
                await _releasePublishingStatusService.UpdateContentStageAsync(releaseVersionId: releaseVersionId,
                    releaseStatusId: releaseStatusId,
                    ReleasePublishingStatusContentStage.Queued);
            });
    }

    public Task QueuePublishReleaseFilesMessage(Guid releaseVersionId, Guid releaseStatusId)
    {
        return QueuePublishReleaseFilesMessage(new[] { (releaseVersionId, releaseStatusId) });
    }

    public async Task QueuePublishReleaseFilesMessage(
        IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases)
    {
        var releasesList = releases.ToList();
        _logger.LogInformation(
            "Queuing files message for release versions: {ReleaseVersionIds}",
            string.Join(", ", releasesList.Select(tuple => tuple.ReleaseVersionId)));
        await _storageQueueService.AddMessageAsync(
            PublishReleaseFilesQueue, new PublishReleaseFilesMessage(releasesList));
        foreach (var (releaseVersionId, releaseStatusId) in releasesList)
        {
            await _releasePublishingStatusService.UpdateFilesStageAsync(releaseVersionId: releaseVersionId,
                releaseStatusId: releaseStatusId,
                ReleasePublishingStatusFilesStage.Queued);
        }
    }
}
