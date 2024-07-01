#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublishingServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task RetryReleasePublishing()
        {
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var publisherQueueServiceClient = new Mock<IPublisherQueueServiceClient>(MockBehavior.Strict);

            publisherQueueServiceClient.Setup(
                    mock => mock.SendMessageAsJson(
                        PublisherQueues.RetryReleasePublishingQueue,
                        new RetryReleasePublishingMessage(releaseVersion.Id),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    publisherQueueServiceClient: publisherQueueServiceClient.Object);

                var result = await publishingService.RetryReleasePublishing(releaseVersion.Id);

                publisherQueueServiceClient.Verify(
                    mock => mock.SendMessageAsJson(
                        PublisherQueues.RetryReleasePublishingQueue,
                        new RetryReleasePublishingMessage(releaseVersion.Id),
                        CancellationToken.None),
                    Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(publisherQueueServiceClient);
        }

        [Fact]
        public async Task RetryReleasePublishing_ReleaseNotFound()
        {
            await using var context = InMemoryApplicationDbContext();

            var publishingService = BuildPublishingService(contentDbContext: context);

            var result = await publishingService.RetryReleasePublishing(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task ReleaseChanged()
        {
            ReleaseStatus releaseStatus = _fixture.DefaultReleaseStatus();
            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithReleaseStatuses(new[] { releaseStatus });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var publisherQueueServiceClient = new Mock<IPublisherQueueServiceClient>(MockBehavior.Strict);

            var releasePublishingKey = new ReleasePublishingKey(releaseVersion.Id, releaseStatus.Id);

            var expectedMessage = new NotifyChangeMessage(true, releasePublishingKey);

            publisherQueueServiceClient.Setup(mock =>
                    mock.SendMessageAsJson(PublisherQueues.NotifyChangeQueue, expectedMessage, CancellationToken.None))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    publisherQueueServiceClient: publisherQueueServiceClient.Object);

                var result = await publishingService.ReleaseChanged(releasePublishingKey, immediate: true);

                publisherQueueServiceClient.Verify(
                    mock => mock.SendMessageAsJson(PublisherQueues.NotifyChangeQueue,
                        expectedMessage,
                        CancellationToken.None),
                    Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(publisherQueueServiceClient);
        }

        [Fact]
        public async Task ReleaseChanged_ReleaseNotFound()
        {
            await using var context = InMemoryApplicationDbContext();

            var publishingService = BuildPublishingService(contentDbContext: context);

            var result =
                await publishingService.ReleaseChanged(new ReleasePublishingKey(Guid.NewGuid(), Guid.NewGuid()),
                    immediate: true);

            result.AssertNotFound();
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

            var publisherQueueServiceClient = new Mock<IPublisherQueueServiceClient>(MockBehavior.Strict);

            publisherQueueServiceClient.Setup(
                    mock => mock.SendMessageAsJson(PublisherQueues.PublishMethodologyFilesQueue,
                        new PublishMethodologyFilesMessage(methodologyVersion.Id),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publishingService = BuildPublishingService(contentDbContext: context,
                    publisherQueueServiceClient: publisherQueueServiceClient.Object);

                var result = await publishingService
                    .PublishMethodologyFiles(methodologyVersion.Id);

                publisherQueueServiceClient.Verify(
                    mock => mock.SendMessageAsJson(PublisherQueues.PublishMethodologyFilesQueue,
                        new PublishMethodologyFilesMessage(methodologyVersion.Id),
                        CancellationToken.None),
                    Times.Once());

                result.AssertRight();
            }

            VerifyAllMocks(publisherQueueServiceClient);
        }

        [Fact]
        public async Task PublishMethodologyFiles_MethodologyNotFound()
        {
            await using var context = InMemoryApplicationDbContext();

            var publishingService = BuildPublishingService(contentDbContext: context);

            var result = await publishingService
                .PublishMethodologyFiles(Guid.NewGuid());

            result.AssertNotFound();
        }

        private static PublishingService BuildPublishingService(
            ContentDbContext contentDbContext,
            IPublisherQueueServiceClient? publisherQueueServiceClient = null,
            IUserService? userService = null)
        {
            return new PublishingService(
                contentDbContext,
                publisherQueueServiceClient ?? Mock.Of<IPublisherQueueServiceClient>(MockBehavior.Strict),
                userService ?? AlwaysTrueUserService().Object,
                new Mock<ILogger<PublishingService>>().Object);
        }
    }
}
