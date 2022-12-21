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
            var release = new Release
            {
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Releases.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(
                    mock => mock.AddMessageAsync(RetryReleasePublishingQueue,
                        It.Is<RetryReleasePublishingMessage>(message =>
                            message.ReleaseId == release.Id)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService.RetryReleasePublishing(release.Id);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(RetryReleasePublishingQueue,
                        It.Is<RetryReleasePublishingMessage>(message =>
                            message.ReleaseId == release.Id)), Times.Once());

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
            var release = new Release();
            var releaseStatus = new ReleaseStatus
            {
                Release = release
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Releases.AddAsync(release);
                await context.ReleaseStatus.AddAsync(releaseStatus);
                await context.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            storageQueueService.Setup(
                    mock => mock.AddMessageAsync(NotifyChangeQueue,
                        It.Is<NotifyChangeMessage>(message =>
                            message.ReleaseId == release.Id
                            && message.ReleaseStatusId == releaseStatus.Id
                            && message.Immediate)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService
                    .ReleaseChanged(release.Id, releaseStatus.Id, true);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(NotifyChangeQueue,
                        It.Is<NotifyChangeMessage>(message =>
                            message.ReleaseId == release.Id
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
