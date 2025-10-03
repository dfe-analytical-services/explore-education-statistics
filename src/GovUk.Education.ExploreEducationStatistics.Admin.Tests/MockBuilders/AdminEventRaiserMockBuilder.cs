#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Events;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class AdminEventRaiserMockBuilder
{
    private readonly Mock<IAdminEventRaiser> _mock = new(MockBehavior.Strict);
    private readonly List<InvokeArguments> _invocations = [];

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnReleaseSlugChanged = m =>
        m.OnReleaseSlugChanged(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<bool>()
        );

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnPublicationArchived = m =>
        m.OnPublicationArchived(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>());

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnPublicationChanged = m =>
        m.OnPublicationChanged(It.IsAny<Publication>());

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnPublicationDeleted = m =>
        m.OnPublicationDeleted(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LatestPublishedReleaseInfo?>());

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnPublicationLatestPublishedReleaseReordered =
        m =>
            m.OnPublicationLatestPublishedReleaseReordered(It.IsAny<Publication>(), It.IsAny<Guid>(), It.IsAny<Guid>());

    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnPublicationRestored = m =>
        m.OnPublicationRestored(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>());

    public IAdminEventRaiser Build() => _mock.Object;

    public AdminEventRaiserMockBuilder()
    {
        _mock.Setup(m => m.OnThemeUpdated(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        _mock.Setup(OnReleaseSlugChanged).Returns(Task.CompletedTask);

        _mock.Setup(OnPublicationArchived).Returns(Task.CompletedTask);

        _mock.Setup(OnPublicationChanged).Returns(Task.CompletedTask);

        _mock.Setup(OnPublicationDeleted).Returns(Task.CompletedTask);

        _mock
            .Setup(m =>
                m.OnPublicationLatestPublishedReleaseReordered(
                    It.IsAny<Publication>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()
                )
            )
            .Callback(
                (Publication publication, Guid oldLatestPublishedReleaseId, Guid oldLatestPublishedReleaseVersionId) =>
                    _invocations.Add(
                        new InvokeArguments(
                            publication,
                            oldLatestPublishedReleaseId,
                            oldLatestPublishedReleaseVersionId
                        )
                    )
            )
            .Returns(Task.CompletedTask);

        _mock.Setup(OnPublicationRestored).Returns(Task.CompletedTask);
    }

    private record InvokeArguments(
        Publication Publication,
        Guid OldLatestPublishedReleaseId,
        Guid OldLatestPublishedReleaseVersionId
    ) { }

    public class Asserter(AdminEventRaiserMockBuilder mockBuilder)
    {
        public void OnThemeUpdatedWasRaised(Func<Theme, bool>? predicate = null) =>
            mockBuilder._mock.Verify(
                m => m.OnThemeUpdated(It.Is<Theme>(t => predicate == null || predicate(t))),
                Times.Once
            );

        public void OnReleaseSlugChangedWasRaised(
            Guid? expectedReleaseId = null,
            string? expectedNewReleaseSlug = null,
            Guid? expectedPublicationId = null,
            string? expectedPublicationSlug = null,
            bool? expectedIsPublicationArchived = null
        ) =>
            mockBuilder._mock.Verify(
                m =>
                    m.OnReleaseSlugChanged(
                        It.Is<Guid>(releaseId => expectedReleaseId == null || releaseId == expectedReleaseId),
                        It.Is<string>(newReleaseSlug =>
                            expectedNewReleaseSlug == null || newReleaseSlug == expectedNewReleaseSlug
                        ),
                        It.Is<Guid>(publicationId =>
                            expectedPublicationId == null || publicationId == expectedPublicationId
                        ),
                        It.Is<string>(publicationSlug =>
                            expectedPublicationSlug == null || publicationSlug == expectedPublicationSlug
                        ),
                        It.Is<bool>(isPublicationArchived =>
                            expectedIsPublicationArchived == null
                            || isPublicationArchived == expectedIsPublicationArchived
                        )
                    ),
                Times.Once
            );

        public void OnReleaseSlugChangedWasNotRaised() => mockBuilder._mock.Verify(OnReleaseSlugChanged, Times.Never);

        public void OnPublicationArchivedWasRaised(
            Guid? publicationId = null,
            string? publicationSlug = null,
            Guid? supersededByPublicationId = null
        ) =>
            mockBuilder._mock.Verify(
                m =>
                    m.OnPublicationArchived(
                        It.Is<Guid>(actual => publicationId == null || actual == publicationId),
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<Guid>(actual => supersededByPublicationId == null || actual == supersededByPublicationId)
                    ),
                Times.Once
            );

        public void OnPublicationArchivedWasNotRaised() => mockBuilder._mock.Verify(OnPublicationArchived, Times.Never);

        public void OnPublicationChangedWasRaised(Publication publication)
        {
            var expectedEvent = new PublicationChangedEvent(
                publication.Id,
                publication.Slug,
                publication.Title,
                publication.Summary,
                publication.IsArchived()
            );

            mockBuilder._mock.Verify(
                m =>
                    m.OnPublicationChanged(
                        It.Is<Publication>(p =>
                            new PublicationChangedEvent(p.Id, p.Slug, p.Title, p.Summary, p.IsArchived())
                            == expectedEvent
                        )
                    ),
                Times.Once
            );
        }

        private void OnPublicationChangedWasNotRaised() => mockBuilder._mock.Verify(OnPublicationChanged, Times.Never);

        public void OnPublicationDeletedWasRaised(
            Guid? publicationId = null,
            string? publicationSlug = null,
            LatestPublishedReleaseInfo? latestPublishedRelease = null
        ) =>
            mockBuilder._mock.Verify(
                m =>
                    m.OnPublicationDeleted(
                        It.Is<Guid>(actual => publicationId == null || actual == publicationId),
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<LatestPublishedReleaseInfo?>(actual =>
                            latestPublishedRelease == null || actual == latestPublishedRelease
                        )
                    ),
                Times.Once
            );

        private void OnPublicationDeletedWasNotRaised() => mockBuilder._mock.Verify(OnPublicationDeleted, Times.Never);

        public void OnPublicationLatestPublishedReleaseReorderedWasRaised(
            Publication publication,
            Guid previousReleaseId,
            Guid previousReleaseVersionId
        )
        {
            var expectedEvent = new PublicationLatestPublishedReleaseReorderedEvent(
                publication.Id,
                publication.Title,
                publication.Slug,
                publication.LatestPublishedReleaseVersion?.ReleaseId
                    ?? throw new ArgumentException(
                        "Publication does not have latest published release version child object.",
                        nameof(publication)
                    ),
                publication.LatestPublishedReleaseVersionId!.Value,
                previousReleaseId,
                previousReleaseVersionId,
                publication.IsArchived()
            );

            Xunit.Assert.Single(
                mockBuilder._invocations,
                inv =>
                    new PublicationLatestPublishedReleaseReorderedEvent(
                        inv.Publication.Id,
                        inv.Publication.Title,
                        inv.Publication.Slug,
                        inv.Publication.LatestPublishedReleaseVersion!.ReleaseId,
                        inv.Publication.LatestPublishedReleaseVersionId!.Value,
                        inv.OldLatestPublishedReleaseId,
                        inv.OldLatestPublishedReleaseVersionId,
                        inv.Publication.IsArchived()
                    ) == expectedEvent
            );
        }

        private void OnPublicationLatestPublishedReleaseReorderedWasNotRaised() =>
            mockBuilder._mock.Verify(OnPublicationLatestPublishedReleaseReordered, Times.Never);

        public void OnPublicationRestoredWasRaised(
            Guid? publicationId = null,
            string? publicationSlug = null,
            Guid? previousSupersededByPublicationId = null
        ) =>
            mockBuilder._mock.Verify(
                m =>
                    m.OnPublicationRestored(
                        It.Is<Guid>(actual => publicationId == null || actual == publicationId),
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<Guid>(actual =>
                            previousSupersededByPublicationId == null || actual == previousSupersededByPublicationId
                        )
                    ),
                Times.Once
            );

        public void OnPublicationRestoredWasNotRaised() => mockBuilder._mock.Verify(OnPublicationRestored, Times.Never);

        public void OnPublicationChangedEventsNotRaised()
        {
            OnPublicationArchivedWasNotRaised();
            OnPublicationChangedWasNotRaised();
            OnPublicationDeletedWasNotRaised();
            OnPublicationLatestPublishedReleaseReorderedWasNotRaised();
            OnPublicationRestoredWasNotRaised();
        }
    }

    public Asserter Assert => new(this);
}
