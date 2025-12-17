using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public abstract class NotificationsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class NotifySubscribersTests : NotificationsServiceTests
    {
        [Fact]
        public async Task NotifySubscribersTrueForSomeReleaseVersions_SubscribersNotifiedWhenTrue()
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
                new Update { Created = DateTime.UtcNow, Reason = "latest update note" },
                new Update { Created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)), Reason = "old update note" },
            ];

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2, publication3);
                await contentDbContext.SaveChangesAsync();
            }

            var notifierClient = new Mock<INotifierClient>(MockBehavior.Strict);
            notifierClient
                .Setup(mock =>
                    mock.NotifyPublicationSubscribers(
                        It.IsAny<IReadOnlyList<ReleaseNotificationMessage>>(),
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext,
                    notifierClient: notifierClient.Object
                );

                await notificationsService.NotifySubscribersIfApplicable([
                    publication1ReleaseVersion0.Id,
                    publication2ReleaseVersion0.Id,
                    publication3ReleaseVersion1.Id,
                ]);

                notifierClient.Verify(
                    mock =>
                        mock.NotifyPublicationSubscribers(
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
                                },
                            },
                            CancellationToken.None
                        ),
                    Times.Once
                );
            }

            VerifyAllMocks(notifierClient);
        }

        [Fact]
        public async Task NoReleaseVersions()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var notificationsService = BuildNotificationsService(contentDbContext);

            await notificationsService.NotifySubscribersIfApplicable([]);

            // No interaction with the notifier client is expected since no releases are being published.
        }
    }

    public class SendReleasePublishingFeedbackTests : NotificationsServiceTests
    {
        [Fact]
        public async Task AllExpectedUsersAreSentFeedbackEmails()
        {
            var (affectedPublication, otherPublication) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1))
                .GenerateTuple2();

            var releaseVersionBeingPublished = affectedPublication.Releases[0].Versions[0];

            var affectedPublicationTeam = _dataFixture
                .DefaultUserPublicationRole()
                .WithPublication(affectedPublication)
                .ForIndex(
                    0,
                    s =>
                        s.SetRole(PublicationRole.Owner)
                            .SetUser(_dataFixture.DefaultUser().WithEmail("affected-publication-owner@example.com"))
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetRole(PublicationRole.Allower)
                            .SetUser(_dataFixture.DefaultUser().WithEmail("affected-publication-approver1@example.com"))
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetRole(PublicationRole.Allower)
                            .SetUser(_dataFixture.DefaultUser().WithEmail("affected-publication-approver2@example.com"))
                )
                .GenerateList();

            // Only users associated with the affected publication, and with active accounts, should receive emails
            var otherPublicationTeam = _dataFixture
                .DefaultUserPublicationRole()
                .WithRole(PublicationRole.Owner)
                // Active user, but different publication
                .ForIndex(
                    0,
                    s =>
                        s.SetPublication(otherPublication)
                            .SetUser(_dataFixture.DefaultUser().WithEmail("other-publication-owner@example.com"))
                )
                // Affected publication, but Pending User Invite
                .ForIndex(
                    1,
                    s =>
                        s.SetPublication(affectedPublication)
                            .SetUser(
                                _dataFixture
                                    .DefaultUserWithPendingInvite()
                                    .WithEmail("affected-publication-pending-invite@example.com")
                            )
                )
                // Affected publication, but Expired User Invite
                .ForIndex(
                    2,
                    s =>
                        s.SetPublication(affectedPublication)
                            .SetUser(
                                _dataFixture
                                    .DefaultUserWithExpiredInvite()
                                    .WithEmail("affected-publication-expired-invite@example.com")
                            )
                )
                // Affected publication, but Soft Deleted User
                .ForIndex(
                    3,
                    s =>
                        s.SetPublication(affectedPublication)
                            .SetUser(
                                _dataFixture
                                    .DefaultSoftDeletedUser()
                                    .WithEmail("affected-publication-soft-deleted-user@example.com")
                            )
                )
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(affectedPublication, otherPublication);
                contentDbContext.UserPublicationRoles.AddRange(affectedPublicationTeam);
                contentDbContext.UserPublicationRoles.AddRange(otherPublicationTeam);
                await contentDbContext.SaveChangesAsync();
            }

            var notifierClient = new Mock<INotifierClient>(MockBehavior.Strict);

            notifierClient
                .Setup(mock =>
                    mock.NotifyReleasePublishingFeedbackUsers(
                        It.IsAny<IReadOnlyList<ReleasePublishingFeedbackMessage>>(),
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext,
                    notifierClient: notifierClient.Object
                );

                await notificationsService.SendReleasePublishingFeedbackEmails([releaseVersionBeingPublished.Id]);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var feedbackEntries = contentDbContext.ReleasePublishingFeedback.ToList();

                Assert.Equal(3, feedbackEntries.Count);

                var ownerFeedbackEntry = feedbackEntries.Single(f => f.UserPublicationRole == PublicationRole.Owner);

                AssertNewFeedbackRecordCreatedOk(
                    feedbackEntry: ownerFeedbackEntry,
                    expectedReleaseVersionId: releaseVersionBeingPublished.Id,
                    expectedRole: PublicationRole.Owner,
                    expectedReleaseTimePeriod: releaseVersionBeingPublished.Release.Title,
                    expectedPublicationTitle: releaseVersionBeingPublished.Release.Publication.Title
                );

                var approverFeedbackEntry1 = feedbackEntries.First(f =>
                    f.UserPublicationRole == PublicationRole.Allower
                );

                AssertNewFeedbackRecordCreatedOk(
                    feedbackEntry: approverFeedbackEntry1,
                    expectedReleaseVersionId: releaseVersionBeingPublished.Id,
                    expectedRole: PublicationRole.Allower,
                    expectedReleaseTimePeriod: releaseVersionBeingPublished.Release.Title,
                    expectedPublicationTitle: releaseVersionBeingPublished.Release.Publication.Title
                );

                var approverFeedbackEntry2 = feedbackEntries.Last(f =>
                    f.UserPublicationRole == PublicationRole.Allower
                );

                AssertNewFeedbackRecordCreatedOk(
                    feedbackEntry: approverFeedbackEntry2,
                    expectedReleaseVersionId: releaseVersionBeingPublished.Id,
                    expectedRole: PublicationRole.Allower,
                    expectedReleaseTimePeriod: releaseVersionBeingPublished.Release.Title,
                    expectedPublicationTitle: releaseVersionBeingPublished.Release.Publication.Title
                );

                notifierClient.Verify(
                    mock =>
                        mock.NotifyReleasePublishingFeedbackUsers(
                            new List<ReleasePublishingFeedbackMessage>
                            {
                                new(ownerFeedbackEntry.Id, "affected-publication-owner@example.com"),
                                new(approverFeedbackEntry1.Id, "affected-publication-approver1@example.com"),
                                new(approverFeedbackEntry2.Id, "affected-publication-approver2@example.com"),
                            },
                            CancellationToken.None
                        ),
                    Times.Once
                );
            }

            VerifyAllMocks(notifierClient);
        }

        [Fact]
        public async Task MultipleReleaseVersionPublished_MultipleEmailsSentToTeam()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(2));

            var release1Version = publication.Releases[0].Versions[0];
            var release2Version = publication.Releases[1].Versions[0];

            UserPublicationRole publicationOwner = _dataFixture
                .DefaultUserPublicationRole()
                .WithPublication(publication)
                .WithRole(PublicationRole.Owner)
                .WithUser(_dataFixture.DefaultUser().WithEmail("publication-owner@example.com"));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication);
                contentDbContext.UserPublicationRoles.AddRange(publicationOwner);
                await contentDbContext.SaveChangesAsync();
            }

            var notifierClient = new Mock<INotifierClient>(MockBehavior.Strict);

            notifierClient
                .Setup(mock =>
                    mock.NotifyReleasePublishingFeedbackUsers(
                        It.IsAny<IReadOnlyList<ReleasePublishingFeedbackMessage>>(),
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext,
                    notifierClient: notifierClient.Object
                );

                await notificationsService.SendReleasePublishingFeedbackEmails([
                    release1Version.Id,
                    release2Version.Id,
                ]);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var feedbackEntries = contentDbContext.ReleasePublishingFeedback.ToList();

                Assert.Equal(2, feedbackEntries.Count);

                var release1OwnerFeedbackEntry = feedbackEntries.Single(f => f.ReleaseVersionId == release1Version.Id);

                AssertNewFeedbackRecordCreatedOk(
                    feedbackEntry: release1OwnerFeedbackEntry,
                    expectedReleaseVersionId: release1Version.Id,
                    expectedRole: PublicationRole.Owner,
                    expectedReleaseTimePeriod: release1Version.Release.Title,
                    expectedPublicationTitle: release1Version.Release.Publication.Title
                );

                var release2OwnerFeedbackEntry = feedbackEntries.Single(f => f.ReleaseVersionId == release2Version.Id);

                AssertNewFeedbackRecordCreatedOk(
                    feedbackEntry: release2OwnerFeedbackEntry,
                    expectedReleaseVersionId: release2Version.Id,
                    expectedRole: PublicationRole.Owner,
                    expectedReleaseTimePeriod: release2Version.Release.Title,
                    expectedPublicationTitle: release2Version.Release.Publication.Title
                );

                notifierClient.Verify(
                    mock =>
                        mock.NotifyReleasePublishingFeedbackUsers(
                            new List<ReleasePublishingFeedbackMessage>
                            {
                                new(release1OwnerFeedbackEntry.Id, "publication-owner@example.com"),
                                new(release2OwnerFeedbackEntry.Id, "publication-owner@example.com"),
                            },
                            CancellationToken.None
                        ),
                    Times.Once
                );
            }

            VerifyAllMocks(notifierClient);
        }

        [Fact]
        public async Task OldApproverRoleAndDrafterRole_NoEmailsSent()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            var filteredOutPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithPublication(publication)
                .ForIndex(
                    0,
                    s =>
                        s.SetRole(PublicationRole.Approver)
                            .SetUser(
                                _dataFixture.DefaultUser().WithEmail("affected-publication-old-approver@example.com")
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetRole(PublicationRole.Drafter)
                            .SetUser(_dataFixture.DefaultUser().WithEmail("affected-publication-drafter@example.com"))
                )
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication);
                contentDbContext.UserPublicationRoles.AddRange(filteredOutPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var notifierClient = new Mock<INotifierClient>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var notificationsService = BuildNotificationsService(
                    contentDbContext,
                    notifierClient: notifierClient.Object
                );

                await notificationsService.SendReleasePublishingFeedbackEmails([
                    publication.Releases[0].Versions[0].Id,
                ]);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var feedbackEntries = contentDbContext.ReleasePublishingFeedback.ToList();

                Assert.Empty(feedbackEntries);
            }
        }

        private static void AssertNewFeedbackRecordCreatedOk(
            ReleasePublishingFeedback feedbackEntry,
            Guid expectedReleaseVersionId,
            PublicationRole expectedRole,
            string expectedReleaseTimePeriod,
            string expectedPublicationTitle
        )
        {
            feedbackEntry.Created.AssertUtcNow();
            Assert.NotEmpty(feedbackEntry.EmailToken);
            Assert.Null(feedbackEntry.Response);
            Assert.Null(feedbackEntry.AdditionalFeedback);
            Assert.Equal(expectedReleaseVersionId, feedbackEntry.ReleaseVersionId);
            Assert.Equal(expectedRole, feedbackEntry.UserPublicationRole);
            Assert.Equal(expectedReleaseTimePeriod, feedbackEntry.ReleaseTitle);
            Assert.Equal(expectedPublicationTitle, feedbackEntry.PublicationTitle);
        }
    }

    private static NotificationsService BuildNotificationsService(
        ContentDbContext contentDbContext,
        INotifierClient? notifierClient = null
    )
    {
        return new NotificationsService(
            contentDbContext,
            notifierClient ?? Mock.Of<INotifierClient>(MockBehavior.Strict)
        );
    }
}
