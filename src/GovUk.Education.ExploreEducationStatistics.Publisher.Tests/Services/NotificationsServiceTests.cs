#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Model.NotifierQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class NotificationsServiceTests
    {
        [Fact]
        public async Task NotifySubscribersIfApplicable()
        {
            var release1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub1 title",
                    Slug = "pub1-slug",
                },
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = DateTime.UtcNow,
                        NotifySubscribers = true,
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Created = DateTime.UtcNow.AddDays(-1),
                        NotifySubscribers = false,
                    },
                },
            };

            var release2 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub2 title",
                    Slug = "pub2-slug",
                },
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = DateTime.UtcNow,
                        NotifySubscribers = false,
                    },
                },
            };

            var release3 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub3 title",
                    Slug = "pub3-slug",
                },
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = DateTime.UtcNow,
                        NotifySubscribers = true,
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2, release3);
                await contentDbContext.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);
            storageQueueService.Setup(mock => mock.AddMessages(
                PublicationQueue,
                It.IsAny<List<PublicationNotificationMessage>>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext, storageQueueService.Object);

                await notificationsService.NotifySubscribersIfApplicable(release1.Id, release2.Id, release3.Id);

                storageQueueService.Verify(mock => mock.AddMessages(
                    PublicationQueue,
                new List<PublicationNotificationMessage>
                    {
                        new()
                        {
                            Name = release1.Publication.Title,
                            PublicationId = release1.Publication.Id,
                            Slug = release1.Publication.Slug,
                        },
                        new()
                        {
                            Name = release3.Publication.Title,
                            PublicationId = release3.Publication.Id,
                            Slug = release3.Publication.Slug,
                        },
                    }), Times.Once);
            }

            MockUtils.VerifyAllMocks(storageQueueService);
        }

        [Fact]
        public async Task NotifySubscribersIfApplicable_NotifyOnSet()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub title",
                    Slug = "pub-slug",
                },
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = DateTime.UtcNow,
                        NotifySubscribers = true,
                        NotifiedOn = DateTime.UtcNow,
                    },
                },
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(contentDbContext);

                await notificationsService.NotifySubscribersIfApplicable(release.Id);

                // No interaction with storage queue service expected as NotifyOn is set
                // in ReleaseStatus.
            }
        }

        [Fact]
        public async Task NotifySubscribersIfApplicable_NoReleaseIds()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var notificationsService = BuildNotificationsService(contentDbContext);

            await notificationsService.NotifySubscribersIfApplicable();

            // No interaction with the storage queue service is expected
            // since no releases are being published.
        }

        private static NotificationsService BuildNotificationsService(
            ContentDbContext contentDbContext,
            IStorageQueueService? storageQueueService = null)
        {
            return new (
                contentDbContext,
                storageQueueService ?? Mock.Of<IStorageQueueService>(MockBehavior.Strict));
        }
    }
}
