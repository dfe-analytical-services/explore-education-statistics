using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class NotificationsServiceTests
    {
        [Fact]
        public async Task NotifySubscribersIfApplicable()
        {
            var release1Version = new ReleaseVersion
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

            var release2Version = new ReleaseVersion
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

            var amendedReleaseVersion = new ReleaseVersion
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
                contentDbContext.ReleaseVersions.AddRange(release1Version, release2Version, amendedReleaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var notifierClient = new Mock<INotifierClient>(MockBehavior.Strict);
            notifierClient.Setup(mock => mock.NotifyPublicationSubscribers(
                    It.IsAny<IReadOnlyList<ReleaseNotificationMessage>>(),
                    CancellationToken.None))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext,
                    notifierClient: notifierClient.Object);

                await notificationsService.NotifySubscribersIfApplicable(release1Version.Id,
                    release2Version.Id,
                    amendedReleaseVersion.Id);

                notifierClient.Verify(mock => mock.NotifyPublicationSubscribers(
                        ItIs.DeepEqualTo(new List<ReleaseNotificationMessage>
                        {
                            new()
                            {
                                PublicationId = release1Version.Publication.Id,
                                PublicationName = release1Version.Publication.Title,
                                PublicationSlug = release1Version.Publication.Slug,
                                ReleaseName = release1Version.Title,
                                ReleaseSlug = release1Version.Slug,
                                Amendment = false,
                                UpdateNote = "No update note provided.",
                            },
                            new()
                            {
                                PublicationId = amendedReleaseVersion.Publication.Id,
                                PublicationName = amendedReleaseVersion.Publication.Title,
                                PublicationSlug = amendedReleaseVersion.Publication.Slug,
                                ReleaseName = amendedReleaseVersion.Title,
                                ReleaseSlug = amendedReleaseVersion.Slug,
                                Amendment = true,
                                UpdateNote = "latest update note",
                            },
                        }),
                        CancellationToken.None),
                    Times.Once);
            }

            MockUtils.VerifyAllMocks(notifierClient);
        }

        [Fact]
        public async Task NotifySubscribersIfApplicable_NoReleaseIds()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var notificationsService = BuildNotificationsService(contentDbContext);

            await notificationsService.NotifySubscribersIfApplicable();

            // No interaction with the queue client is expected since no releases are being published.
        }

        private static NotificationsService BuildNotificationsService(
            ContentDbContext contentDbContext,
            INotifierClient? notifierClient = null)
        {
            return new NotificationsService(
                contentDbContext,
                notifierClient ?? Mock.Of<INotifierClient>(MockBehavior.Strict));
        }
    }
}
