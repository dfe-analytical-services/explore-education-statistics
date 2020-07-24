using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.RetryStage;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublishingServiceTests
    {
        [Fact]
        public void RetryStage()
        {
            var mocks = Mocks();

            var release = new Release
            {
                Id = new Guid("af032e3c-67c2-4562-9717-9a305a468263"),
                Status = ReleaseStatus.Approved,
                Version = 0,
                PreviousVersionId = new Guid("af032e3c-67c2-4562-9717-9a305a468263")
            };

            using (var context = InMemoryApplicationDbContext("RetryContentAndPublishing"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("RetryContentAndPublishing"))
            {
                var publishingService = BuildPublishingService(context, mocks);
                var result = publishingService.RetryStage(release.Id, ContentAndPublishing).Result.Right;

                mocks.StorageQueueService.Verify(
                    mock => mock.AddMessagesAsync(RetryStageQueue,
                        It.Is<RetryStageMessage>(message =>
                            message.ReleaseId == release.Id && message.Stage == ContentAndPublishing)), Times.Once());

                Assert.True(result);
            }
        }

        private static PublishingService BuildPublishingService(ContentDbContext context,
            (Mock<IStorageQueueService> storageQueueService,
                Mock<IUserService> userService,
                Mock<ILogger<PublishingService>> logger) mocks)
        {
            var (storageQueueService, userService, logger) = mocks;

            return new PublishingService(new PersistenceHelper<ContentDbContext>(context),
                storageQueueService.Object,
                userService.Object,
                logger.Object);
        }

        private static (Mock<IStorageQueueService> StorageQueueService,
            Mock<IUserService> UserService,
            Mock<ILogger<PublishingService>> Logger) Mocks()
        {
            return (
                new Mock<IStorageQueueService>(),
                MockUtils.AlwaysTrueUserService(),
                new Mock<ILogger<PublishingService>>());
        }
    }
}