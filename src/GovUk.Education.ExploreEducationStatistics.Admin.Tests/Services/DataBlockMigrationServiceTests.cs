#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockMigrationServiceTests
    {
        [Fact]
        public async Task MigrateAll_FailsWithNullMessageCount()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(mock =>
                    mock.GetApproximateMessageCount(MigrateDataBlocksQueue))
                .ReturnsAsync((int?)null);

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);

            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            MockUtils.VerifyAllMocks(storageQueueService);

            var left = result.AssertLeft();
            Assert.IsAssignableFrom<BadRequestObjectResult>(left);
        }

        [Fact]
        public async Task MigrateAll_FailsWithNonZeroMessageCount()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(mock =>
                    mock.GetApproximateMessageCount(MigrateDataBlocksQueue))
                .ReturnsAsync(1);

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);

            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            MockUtils.VerifyAllMocks(storageQueueService);

            var left = result.AssertLeft();
            Assert.IsAssignableFrom<BadRequestObjectResult>(left);
        }

        [Fact]
        public async Task MigrateAll_IgnoresMigrated()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(mock =>
                    mock.GetApproximateMessageCount(MigrateDataBlocksQueue))
                .ReturnsAsync(0);

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);

            // Set up a data block which is already flagged as migrated
            await contentDbContext.DataBlocks.AddRangeAsync(new DataBlock
            {
                LocationsMigrated = true
            });
            await contentDbContext.SaveChangesAsync();

            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            MockUtils.VerifyAllMocks(storageQueueService);

            result.AssertRight();
        }

        [Fact]
        public async Task MigrateAll()
        {
            var dataBlocks = new List<DataBlock>
            {
                new()
                {
                    Id = Guid.NewGuid()
                },
                // Ignored
                new()
                {
                  Id = Guid.NewGuid(),
                  LocationsMigrated = true
                },
                new()
                {
                    Id = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(mock =>
                    mock.GetApproximateMessageCount(MigrateDataBlocksQueue))
                .ReturnsAsync(0);

            storageQueueService.Setup(mock =>
                    mock.AddMessages(MigrateDataBlocksQueue, new List<MigrateDataBlockMessage>
                    {
                        new(dataBlocks[0].Id),
                        new(dataBlocks[2].Id)
                    }))
                .Returns(Task.CompletedTask);

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);

            await contentDbContext.DataBlocks.AddRangeAsync(dataBlocks);
            await contentDbContext.SaveChangesAsync();

            var service = SetupService(
                contentDbContext: contentDbContext,
                storageQueueService: storageQueueService.Object);

            var result = await service.MigrateAll();

            MockUtils.VerifyAllMocks(storageQueueService);

            result.AssertRight();
        }

        private static DataBlockMigrationService SetupService(
            ContentDbContext contentDbContext,
            IStorageQueueService? storageQueueService = null,
            IUserService? userService = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                storageQueueService ?? Mock.Of<IStorageQueueService>(MockBehavior.Strict),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                Mock.Of<ILogger<DataBlockMigrationService>>()
            );
        }
    }
}
