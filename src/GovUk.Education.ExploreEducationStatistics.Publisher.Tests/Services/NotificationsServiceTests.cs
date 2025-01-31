using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class NotificationsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task NotifySubscribersIfApplicable()
    {
        var (publication1, publication2, publication3) = _dataFixture
            .DefaultPublication()
            .ForRange(..2, s => s.SetReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]))
            .ForRange(2..3, s => s.SetReleases([_dataFixture.DefaultRelease(publishedVersions: 2)]))
            .GenerateTuple3();

        var publication1Release = publication1.Releases.Single();
        var publication1ReleaseVersion0 = publication1Release.Versions[0];
        publication1ReleaseVersion0.NotifySubscribers = true;

        var publication2Release = publication2.Releases.Single();
        var publication2ReleaseVersion0 = publication2Release.Versions[0];
        publication2ReleaseVersion0.NotifySubscribers = false;

        var publication3Release = publication3.Releases.Single();
        var publication3ReleaseVersion1 = publication3Release.Versions[1];
        publication3ReleaseVersion1.NotifySubscribers = true;
        publication3ReleaseVersion1.Updates =
        [
            new Update
            {
                Created = DateTime.UtcNow,
                Reason = "latest update note"
            },
            new Update
            {
                Created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                Reason = "old update note"
            }
        ];

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.AddRange(publication1, publication2, publication3);
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

            await notificationsService.NotifySubscribersIfApplicable([
                publication1ReleaseVersion0.Id,
                publication2ReleaseVersion0.Id,
                publication3ReleaseVersion1.Id
            ]);

            notifierClient.Verify(mock => mock.NotifyPublicationSubscribers(
                    new List<ReleaseNotificationMessage>
                    {
                        new()
                        {
                            PublicationId = publication1.Id,
                            PublicationName = publication1.Title,
                            PublicationSlug = publication1.Slug,
                            ReleaseName = publication1Release.Title,
                            ReleaseSlug = publication1Release.Slug,
                            Amendment = false,
                            UpdateNote = "No update note provided.",
                        },
                        new()
                        {
                            PublicationId = publication3.Id,
                            PublicationName = publication3.Title,
                            PublicationSlug = publication3.Slug,
                            ReleaseName = publication3Release.Title,
                            ReleaseSlug = publication3Release.Slug,
                            Amendment = true,
                            UpdateNote = "latest update note",
                        }
                    },
                    CancellationToken.None),
                Times.Once);
        }

        MockUtils.VerifyAllMocks(notifierClient);
    }

    [Fact]
    public async Task NotifySubscribersIfApplicable_NoReleaseVersions()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var notificationsService = BuildNotificationsService(contentDbContext);

        await notificationsService.NotifySubscribersIfApplicable([]);

        // No interaction with the notifier client is expected since no releases are being published.
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
