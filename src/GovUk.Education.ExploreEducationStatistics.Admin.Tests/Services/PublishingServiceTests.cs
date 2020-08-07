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

            using (var context = InMemoryApplicationDbContext("RetryReleaseStage"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("RetryReleaseStage"))
            {
                var publishingService = BuildPublishingService(context, mocks);
                var result = publishingService.RetryReleaseStage(release.Id, ContentAndPublishing).Result;

                mocks.StorageQueueService.Verify(
                    mock => mock.AddMessagesAsync(RetryStageQueue,
                        It.Is<RetryStageMessage>(message =>
                            message.ReleaseId == release.Id && message.Stage == ContentAndPublishing)), Times.Once());

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public void ReleaseChanged()
        {
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            using (var context = InMemoryApplicationDbContext("ReleaseChanged"))
            {
                context.Add(publication);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("ReleaseChanged"))
            {
                var publishingService = BuildPublishingService(context, mocks);
                var result = publishingService.ReleaseChanged(publication.Id, true).Result;

                mocks.StorageQueueService.Verify(
                    mock => mock.AddMessagesAsync(PublishPublicationQueue,
                        It.Is<NotifyChangeMessage>(message =>
                            message.ReleaseId == publication.Id && message.Immediate)), Times.Once());

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public void PublicationChanged()
        {
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            using (var context = InMemoryApplicationDbContext("PublicationChanged"))
            {
                context.Add(publication);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PublicationChanged"))
            {
                var publishingService = BuildPublishingService(context, mocks);
                var result = publishingService.PublicationChanged(publication.Id).Result;

                mocks.StorageQueueService.Verify(
                    mock => mock.AddMessagesAsync(PublishPublicationQueue,
                        It.Is<PublishPublicationMessage>(message =>
                            message.PublicationId == publication.Id)), Times.Once());

                Assert.True(result.IsRight);
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