#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
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
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Slug = "2000-01",
                NotifySubscribers = true,
                Version = 0,
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub1 title",
                    Slug = "pub1-slug",
                }
            };

            var release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Slug = "2001-02",
                NotifySubscribers = false,
                Version = 0,
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub2 title",
                    Slug = "pub2-slug",
                }
            };

            var amendedRelease1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2002",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Slug = "2002-03",
                NotifySubscribers = true,
                Version = 1,
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "pub3 title",
                    Slug = "pub3-slug",
                },
                Updates = new List<Update>
                {
                    new()
                    {
                        Created = DateTime.UtcNow,
                        Reason = "latest update note"
                    },
                    new()
                    {
                        Created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                        Reason = "old update note"
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release1, release2, amendedRelease1);
                await contentDbContext.SaveChangesAsync();
            }

            var storageQueueService = new Mock<IStorageQueueService>(MockBehavior.Strict);
            storageQueueService.Setup(mock => mock.AddMessages(
                    ReleaseNotificationQueue,
                    It.IsAny<List<ReleaseNotificationMessage>>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext, storageQueueService.Object);

                await notificationsService.NotifySubscribersIfApplicable(release1.Id, release2.Id, amendedRelease1.Id);

                storageQueueService.Verify(mock => mock.AddMessages(
                    ReleaseNotificationQueue,
                    new List<ReleaseNotificationMessage>
                    {
                        new()
                        {
                            PublicationId = release1.Publication.Id,
                            PublicationName = release1.Publication.Title,
                            PublicationSlug = release1.Publication.Slug,
                            ReleaseName = release1.Title,
                            ReleaseSlug = release1.Slug,
                            Amendment = false,
                            UpdateNote = "No update note provided.",
                        },
                        new()
                        {
                            PublicationId = amendedRelease1.Publication.Id,
                            PublicationName = amendedRelease1.Publication.Title,
                            PublicationSlug = amendedRelease1.Publication.Slug,
                            ReleaseName = amendedRelease1.Title,
                            ReleaseSlug = amendedRelease1.Slug,
                            Amendment = true,
                            UpdateNote = "latest update note",
                        },
                    }), Times.Once);
            }

            MockUtils.VerifyAllMocks(storageQueueService);
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
