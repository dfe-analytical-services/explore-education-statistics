#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

public class PermalinksMigrationFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IStorageQueueService _storageQueueService;

    public PermalinksMigrationFunction(
        BlobServiceClient blobServiceClient,
        IStorageQueueService storageQueueService)
    {
        _blobServiceClient = blobServiceClient;
        _storageQueueService = storageQueueService;
    }

    /// <summary>
    /// Azure Function which enumerates all the permalink blobs in the permalinks storage container,
    /// and queues a message to migrate each individual permalink.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="executionContext"></param>
    /// <param name="logger"></param>
    [FunctionName("PermalinksMigration")]
    public async Task PermalinksMigration(
        [QueueTrigger(PermalinksMigrationQueue)]
        PermalinksMigrationMessage message,
        ExecutionContext executionContext,
        ILogger logger)
    {
        logger.LogInformation("{functionName} triggered", executionContext.FunctionName);

        try
        {
            string? continuationToken = null;

            var blobContainer = _blobServiceClient.GetBlobContainerClient(Permalinks.Name);

            do
            {
                var pages = blobContainer
                    .GetBlobsAsync(BlobTraits.None, prefix: null)
                    .AsPages(continuationToken);

                await foreach (var page in pages)
                {
                    var messages = page.Values.Select(blobItem =>
                    {
                        var name = blobItem.Name;
                        var permalinkId = Guid.Parse(name);
                        return new PermalinkMigrationMessage(permalinkId);
                    }).ToList();

                    await _storageQueueService.AddMessages(PermalinkMigrationQueue, messages);

                    continuationToken = page.ContinuationToken;
                }
            } while (continuationToken != string.Empty);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to enumerate all permalinks");
        }

        logger.LogInformation("{functionName} completed", executionContext.FunctionName);
    }
}
