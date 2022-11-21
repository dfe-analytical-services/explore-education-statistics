#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static Azure.Storage.Blobs.Models.BlobsModelFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationServiceTests
{
    private readonly Guid _subjectId = Guid.NewGuid();
    private readonly DateTime _created = DateTime.UtcNow;
    private const string PublicationTitle = "Test publication";
    private const string DataSetTitle = "Test data set";

    [Fact]
    public async Task EnumerateAllPermalinksForMigration()
    {
        var permalink1Id = Guid.NewGuid();
        var permalink2Id = Guid.NewGuid();
        var permalink3Id = Guid.NewGuid();
        var permalink4Id = Guid.NewGuid();
        var permalink5Id = Guid.NewGuid();
        var permalink6Id = Guid.NewGuid();

        var blobContainerClient = MockBlobContainerClient(Permalinks.Name);
        var blobServiceClient = MockBlobServiceClient(blobContainerClient);
        var storageQueueService = new Mock<IStorageQueueService>(Strict);

        // Set up the container to return two pages of blob results
        // Set up each page to contain multiple permalink blobs

        blobContainerClient.Setup(mock => mock.GetBlobsAsync(
                BlobTraits.None,
                BlobStates.None,
                null,
                CancellationToken.None))
            .Returns(new TestAsyncPageable(new List<Page<BlobItem>>
            {
                Page(
                    BlobItem(permalink1Id.ToString()),
                    BlobItem(permalink2Id.ToString()),
                    BlobItem(permalink3Id.ToString())
                ),
                Page(
                    BlobItem(permalink4Id.ToString()),
                    BlobItem(permalink5Id.ToString()),
                    BlobItem(permalink6Id.ToString())
                )
            }));

        var sequence = new MockSequence();

        // Expect migration messages to be added to the queue in batches.
        // There should be a batch of messages added to the queue per page of blob results.
        // Each batch of messages should contain the permalink id's taken from the blob names on that page.

        storageQueueService.InSequence(sequence).Setup(mock => mock.AddMessages(
            PermalinkMigrationQueue,
            new List<PermalinkMigrationMessage>
            {
                new(permalink1Id),
                new(permalink2Id),
                new(permalink3Id)
            })).Returns(Task.CompletedTask);

        storageQueueService.InSequence(sequence).Setup(mock => mock.AddMessages(
            PermalinkMigrationQueue,
            new List<PermalinkMigrationMessage>
            {
                new(permalink4Id),
                new(permalink5Id),
                new(permalink6Id)
            })).Returns(Task.CompletedTask);

        var service = SetupService(
            blobServiceClient: blobServiceClient.Object,
            storageQueueService: storageQueueService.Object
        );

        await service.EnumerateAllPermalinksForMigration();

        VerifyAllMocks(blobContainerClient, blobServiceClient, storageQueueService);
    }

    [Fact]
    public async Task AddPermalinkToDbFromStorage()
    {
        var permalinkId = Guid.NewGuid();
        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            var permalink = await service.AddPermalinkToDbFromStorage(permalinkId);

            Assert.Equal(permalinkId, permalink.Id);
            Assert.Equal(_created, permalink.Created);
            Assert.Equal(PublicationTitle, permalink.PublicationTitle);
            Assert.Equal(DataSetTitle, permalink.DataSetTitle);
            Assert.Equal(_subjectId, permalink.SubjectId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            Assert.Equal(permalinkId, saved!.Id);
            Assert.Equal(_created, saved.Created);
            Assert.Equal(PublicationTitle, saved.PublicationTitle);
            Assert.Equal(DataSetTitle, saved.DataSetTitle);
            Assert.Equal(_subjectId, saved.SubjectId);
        }
    }

    [Fact]
    public async Task AddPermalinkToDbFromStorage_PermalinkNotFound()
    {
        var permalinkId = Guid.NewGuid();
        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, exists: false);

        await using var contentDbContext = InMemoryContentDbContext();

        var service = SetupService(contentDbContext: contentDbContext,
            blobServiceClient: blobServiceClient.Object
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AddPermalinkToDbFromStorage(permalinkId));

        Assert.Equal($"Blob not found for permalink {permalinkId}", exception.Message);
        Assert.Null(contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId));
    }

    [Fact]
    public async Task AddPermalinkToDbFromStorage_PermalinkAlreadyExists()
    {
        var existingPermalink = new Permalink
        {
            Id = Guid.NewGuid(),
            PublicationTitle = "Existing permalink publication",
            DataSetTitle = "Existing permalink data set",
            SubjectId = Guid.NewGuid(),
            Created = DateTime.UtcNow
        };

        var blobServiceClient = MockBlobServiceClientForPermalink(existingPermalink.Id);

        var contentDbContextId = Guid.NewGuid().ToString();

        // Setup a permalink already in the database
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Permalinks.Add(existingPermalink);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            // Call to add permalink to db from storage should ignore it since it already exists  
            var permalink = await service.AddPermalinkToDbFromStorage(existingPermalink.Id);

            Assert.Equal(existingPermalink.Id, permalink!.Id);
            Assert.Equal(existingPermalink.Created, permalink.Created);
            Assert.Equal(existingPermalink.PublicationTitle, permalink.PublicationTitle);
            Assert.Equal(existingPermalink.DataSetTitle, permalink.DataSetTitle);
            Assert.Equal(existingPermalink.SubjectId, permalink.SubjectId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var found = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == existingPermalink.Id);
            Assert.NotNull(found);

            // Permalink found in the database shouldn't have been updated since it already existed
            Assert.Equal(existingPermalink.Id, found!.Id);
            Assert.Equal(existingPermalink.Created, found.Created);
            Assert.Equal(existingPermalink.PublicationTitle, found.PublicationTitle);
            Assert.Equal(existingPermalink.DataSetTitle, found.DataSetTitle);
            Assert.Equal(existingPermalink.SubjectId, found.SubjectId);
        }
    }

    private Mock<BlobServiceClient> MockBlobServiceClientForPermalink(Guid permalinkId,
        bool exists = true)
    {
        var permalink = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""Created"": ""{_created.ToString("O")}"",
            ""FullTable"": {{
                ""SubjectMeta"": {{
                    ""PublicationName"": ""{PublicationTitle}"",
                    ""SubjectName"": ""{DataSetTitle}""
                }}
            }},
            ""Query"": {{
                ""SubjectId"": ""{_subjectId}""
            }}
        }}";

        var blobClient = MockBlobClient(permalinkId.ToString(), exists);
        var blobContainerClient = MockBlobContainerClient(Permalinks.Name, blobClient);

        if (exists)
        {
            blobClient.Setup(s => s.OpenReadAsync(0, null, null, default))
                .ReturnsAsync(permalink.ToStream());
        }

        return MockBlobServiceClient(blobContainerClient);
    }

    private static Mock<BlobClient> MockBlobClient(string name, bool exists = true)
    {
        var blobClient = new Mock<BlobClient>(Strict);

        blobClient.SetupGet(client => client.Name)
            .Returns(name);

        blobClient.Setup(client => client.ExistsAsync(default))
            .ReturnsAsync(Response.FromValue(exists, null!));

        return blobClient;
    }

    private static Mock<BlobContainerClient> MockBlobContainerClient(string containerName,
        params Mock<BlobClient>[] blobClients)
    {
        var blobContainerClient = new Mock<BlobContainerClient>(Strict);

        blobContainerClient
            .SetupGet(client => client.Name)
            .Returns(containerName);

        foreach (var blobClient in blobClients)
        {
            blobContainerClient.Setup(client => client.GetBlobClient(blobClient.Object.Name))
                .Returns(blobClient.Object);
        }

        return blobContainerClient;
    }

    private static Mock<BlobServiceClient> MockBlobServiceClient(
        params Mock<BlobContainerClient>[] blobContainerClients)
    {
        var blobServiceClient = new Mock<BlobServiceClient>(Strict);

        foreach (var blobContainerClient in blobContainerClients)
        {
            blobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerClient.Object.Name))
                .Returns(blobContainerClient.Object);
        }

        return blobServiceClient;
    }

    private static Page<BlobItem> Page(params BlobItem[] blobItems)
    {
        return Page<BlobItem>.FromValues(blobItems, "", null!);
    }

    private class TestAsyncPageable : AsyncPageable<BlobItem>
    {
        private readonly IEnumerable<Page<BlobItem>> _pages;

        public TestAsyncPageable(IEnumerable<Page<BlobItem>> pages)
        {
            _pages = pages;
        }

        public override async IAsyncEnumerable<Page<BlobItem>> AsPages(
            string? continuationToken = null,
            int? pageSizeHint = null)
        {
            foreach (var page in _pages)
            {
                yield return page;
            }

            await Task.CompletedTask;
        }
    }

    private static PermalinkMigrationService SetupService(
        BlobServiceClient blobServiceClient,
        ContentDbContext? contentDbContext = null,
        IStorageQueueService? storageQueueService = null)
    {
        return new PermalinkMigrationService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            blobServiceClient,
            storageQueueService ?? Mock.Of<IStorageQueueService>(Strict)
        );
    }
}
