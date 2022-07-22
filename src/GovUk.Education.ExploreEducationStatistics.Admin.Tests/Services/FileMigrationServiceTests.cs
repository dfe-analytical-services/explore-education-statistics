#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FileMigrationServiceTests
{
    [Fact]
    public async Task MigrateAll_FailsWithNullMessageCount()
    {
        var storageQueueService = MockStorageQueueService(messageCount: null);

        var service = SetupService(
            contentDbContext: InMemoryContentDbContext(),
            storageQueueService: storageQueueService.Object);

        var result = await service.MigrateAll();

        VerifyAllMocks(storageQueueService);

        result.AssertBadRequest(NullMessageCountForFileMigrationQueue);
    }

    [Fact]
    public async Task MigrateAll_FailsWithNonZeroMessageCount()
    {
        var storageQueueService = MockStorageQueueService(messageCount: 1);

        var service = SetupService(
            contentDbContext: InMemoryContentDbContext(),
            storageQueueService: storageQueueService.Object);

        var result = await service.MigrateAll();

        VerifyAllMocks(storageQueueService);

        result.AssertBadRequest(NonEmptyFileMigrationQueue);
    }

    [Fact]
    public async Task MigrateAll_IgnoresFilesWithContentTypeAndSize()
    {
        // Set up files that already have values for ContentLength and ContentType so don't need migrating
        var files = new List<File>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ContentType = "application/pdf",
                ContentLength = 1024
            },
            new()
            {
                Id = Guid.NewGuid(),
                ContentType = "application/pdf",
                ContentLength = 1024
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddRangeAsync(files);
            await contentDbContext.SaveChangesAsync();
        }

        var storageQueueService = MockStorageQueueService(messageCount: 0);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            // There should be no further interactions with the storage queue service

            VerifyAllMocks(storageQueueService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task MigrateAll_MigratesPartiallyMigratedFilesWithContentTypeOrSize()
    {
        var files = new List<File>
        {
            // Partially migrated, missing ContentType
            new()
            {
                Id = Guid.NewGuid(),
                ContentLength = 1024,
                ContentType = null
            },
            // Ignored as already migrated
            new()
            {
                Id = Guid.NewGuid(),
                ContentType = "application/pdf",
                ContentLength = 1024
            },
            // Partially migrated, missing ContentLength
            new()
            {
                Id = Guid.NewGuid(),
                ContentLength = null,
                ContentType = "application/pdf"
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddRangeAsync(files);
            await contentDbContext.SaveChangesAsync();
        }

        var storageQueueService = MockStorageQueueService(messageCount: 0);

        storageQueueService.Setup(mock =>
                mock.AddMessages(MigrateFilesQueue, new List<MigrateFileMessage>
                {
                    new(files[0].Id),
                    new(files[2].Id)
                }))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            VerifyAllMocks(storageQueueService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task MigrateAll()
    {
        var files = new List<File>
        {
            new()
            {
                Id = Guid.NewGuid()
            },
            // Ignored as already migrated
            new()
            {
                Id = Guid.NewGuid(),
                ContentType = "application/pdf",
                ContentLength = 1024
            },
            new()
            {
                Id = Guid.NewGuid()
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddRangeAsync(files);
            await contentDbContext.SaveChangesAsync();
        }

        var storageQueueService = MockStorageQueueService(messageCount: 0);

        storageQueueService.Setup(mock =>
                mock.AddMessages(MigrateFilesQueue, new List<MigrateFileMessage>
                {
                    new(files[0].Id),
                    new(files[2].Id)
                }))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            VerifyAllMocks(storageQueueService);

            result.AssertRight();
        }
    }

    private static Mock<IStorageQueueService> MockStorageQueueService(int? messageCount)
    {
        var service = new Mock<IStorageQueueService>(MockBehavior.Strict);
        service.Setup(s => s.GetApproximateMessageCount(MigrateFilesQueue))
            .ReturnsAsync(messageCount);
        return service;
    }

    private static FileMigrationService SetupService(
        ContentDbContext contentDbContext,
        IStorageQueueService? storageQueueService = null,
        IUserService? userService = null)
    {
        return new FileMigrationService(
            contentDbContext,
            storageQueueService ?? Mock.Of<IStorageQueueService>(MockBehavior.Strict),
            userService ?? AlwaysTrueUserService().Object,
            Mock.Of<ILogger<FileMigrationService>>()
        );
    }
}
