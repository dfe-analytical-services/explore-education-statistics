#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationService : IPermalinkMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IStorageQueueService _storageQueueService;

    public PermalinkMigrationService(ContentDbContext contentDbContext,
        BlobServiceClient blobServiceClient,
        IStorageQueueService storageQueueService)
    {
        _contentDbContext = contentDbContext;
        _blobServiceClient = blobServiceClient;
        _storageQueueService = storageQueueService;
    }

    public async Task EnumerateAllPermalinksForMigration()
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

    public async Task<Permalink> AddPermalinkToDbFromStorage(Guid permalinkId)
    {
        var existingPermalink = await _contentDbContext.Permalinks.SingleOrDefaultAsync(p => p.Id == permalinkId);
        if (existingPermalink != null)
        {
            // Permalink already exists so no need to migrate it from storage
            return existingPermalink;
        }

        var permalinkFromStorage = await GetPermalinkFromStorage(permalinkId);
        _contentDbContext.Permalinks.Add(permalinkFromStorage);
        await _contentDbContext.SaveChangesAsync();

        return permalinkFromStorage;
    }

    private async Task<Permalink> GetPermalinkFromStorage(Guid permalinkId)
    {
        var blobContainer = _blobServiceClient.GetBlobContainerClient(Permalinks.Name);
        var blob = blobContainer.GetBlobClient(permalinkId.ToString());

        if (!await blob.ExistsAsync())
        {
            throw new InvalidOperationException($"Blob not found for permalink {permalinkId}");
        }

        await using var stream = await blob.OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);

        var jsonObject = await JToken.ReadFromAsync(jsonReader);

        var id = jsonObject.Value<string>("Id");
        var created = jsonObject.Value<DateTime>("Created");

        var subjectId = jsonObject.SelectToken("$.Query.SubjectId")?.ToObject<string>();
        if (subjectId == null)
        {
            throw new InvalidOperationException("Permalink found with no SubjectId");
        }

        var publicationName = jsonObject.SelectToken("$.FullTable.SubjectMeta.PublicationName")?.ToObject<string>();
        if (publicationName == null)
        {
            throw new InvalidOperationException("Permalink found with no PublicationName");
        }

        var subjectName = jsonObject.SelectToken("$.FullTable.SubjectMeta.SubjectName")?.ToObject<string>();
        if (subjectName == null)
        {
            throw new InvalidOperationException("Permalink found with no SubjectName");
        }

        return new Permalink
        {
            Id = Guid.Parse(id),
            PublicationTitle = publicationName,
            DataSetTitle = subjectName,
            ReleaseId = null,
            SubjectId = Guid.Parse(subjectId),
            Created = created
        };
    }
}
