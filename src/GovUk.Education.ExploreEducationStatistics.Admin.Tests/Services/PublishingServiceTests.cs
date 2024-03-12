using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublishingServiceTests
    {
        [Fact]
        public async Task RetryReleasePublishing()
        {
            var releaseVersion = new ReleaseVersion
            {
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(
                    mock => mock.AddMessageAsync(RetryReleasePublishingQueue,
                        It.Is<RetryReleasePublishingMessage>(message =>
                            message.ReleaseVersionId == releaseVersion.Id)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService.RetryReleasePublishing(releaseVersion.Id);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(RetryReleasePublishingQueue,
                        It.Is<RetryReleasePublishingMessage>(message =>
                            message.ReleaseVersionId == releaseVersion.Id)), Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task RetryReleasePublishing_ReleaseNotFound()
        {
            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext())
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService.RetryReleasePublishing(Guid.NewGuid());

                result.AssertNotFound();
            }

            VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task ReleaseChanged()
        {
            var releaseVersion = new ReleaseVersion();
            var releaseStatus = new ReleaseStatus
            {
                ReleaseVersion = releaseVersion
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseStatus.Add(releaseStatus);
                await context.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(
                    mock => mock.AddMessageAsync(NotifyChangeQueue,
                        It.Is<NotifyChangeMessage>(message =>
                            message.ReleaseVersionId == releaseVersion.Id
                            && message.ReleaseStatusId == releaseStatus.Id
                            && message.Immediate)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService
                    .ReleaseChanged(releaseVersion.Id, releaseStatus.Id, true);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(NotifyChangeQueue,
                        It.Is<NotifyChangeMessage>(message =>
                            message.ReleaseVersionId == releaseVersion.Id
                            && message.ReleaseStatusId == releaseStatus.Id
                            && message.Immediate)), Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task ReleaseChanged_ReleaseNotFound()
        {
            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext())
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService
                    .ReleaseChanged(Guid.NewGuid(), Guid.NewGuid(), true);

                result.AssertNotFound();
            }

            VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task PublishMethodologyFiles()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(
                    mock => mock.AddMessageAsync(PublishMethodologyFilesQueue,
                        It.Is<PublishMethodologyFilesMessage>(message =>
                            message.MethodologyId == methodologyVersion.Id)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService
                    .PublishMethodologyFiles(methodologyVersion.Id);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(PublishMethodologyFilesQueue,
                        It.Is<PublishMethodologyFilesMessage>(message =>
                            message.MethodologyId == methodologyVersion.Id)), Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task PublishMethodologyFiles_MethodologyNotFound()
        {
            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext())
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService
                    .PublishMethodologyFiles(Guid.NewGuid());

                result.AssertNotFound();
            }

            VerifyAllMocks(storageQueueService);
        }

        private static PublishingService BuildPublishingService(ContentDbContext contentDbContext,
            IStorageQueueService storageQueueService = null,
            IUserService userService = null,
            ILogger<PublishingService> logger = null)
        {
            return new PublishingService(
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                storageQueueService ?? new Mock<IStorageQueueService>().Object,
                userService ?? AlwaysTrueUserService().Object,
                logger ?? new Mock<ILogger<PublishingService>>().Object);
        }
    }
}
