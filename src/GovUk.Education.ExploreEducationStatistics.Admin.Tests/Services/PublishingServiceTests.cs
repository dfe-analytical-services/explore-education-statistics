using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.RetryStage;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublishingServiceTests
    {
        [Fact]
        public async Task RetryReleaseStage()
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
                    mock => mock.AddMessageAsync(RetryStageQueue,
                        It.Is<RetryStageMessage>(message =>
                            message.ReleaseId == release.Id
                            && message.Stage == ContentAndPublishing)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService.RetryReleaseStage(release.Id, ContentAndPublishing);

                storageQueueService.Verify(
                    mock => mock.AddMessageAsync(RetryStageQueue,
                        It.Is<RetryStageMessage>(message =>
                            message.ReleaseId == release.Id
                            && message.Stage == ContentAndPublishing)), Times.Once());

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task RetryReleaseStage_ReleaseNotFound()
        {
            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext())
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    storageQueueService: storageQueueService.Object);

                var result = await publishingService.RetryReleaseStage(Guid.NewGuid(), ContentAndPublishing);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(storageQueueService);
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

            MockUtils.VerifyAllMocks(storageQueueService);
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

            MockUtils.VerifyAllMocks(storageQueueService);
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

            MockUtils.VerifyAllMocks(storageQueueService);
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

            MockUtils.VerifyAllMocks(storageQueueService);
        }

        private static PublishingService BuildPublishingService(ContentDbContext contentDbContext,
            IStorageQueueService storageQueueService = null,
            IUserService userService = null,
            ILogger<PublishingService> logger = null)
        {
            return new PublishingService(
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                storageQueueService ?? new Mock<IStorageQueueService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                logger ?? new Mock<ILogger<PublishingService>>().Object);
        }
    }
}
