#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class AdminEventRaiserTests
{
    private readonly EventRaiserMockBuilder _eventRaiserMockBuilder = new();

    private IAdminEventRaiser GetSut() =>
        new AdminEventRaiser(_eventRaiserMockBuilder.Build());

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenOnThemeUpdated_ThenEventPublished()
    {
        // ARRANGE
        var sut = GetSut();
        var theme = new ThemeBuilder().Build();

        // ACT
        await sut.OnThemeUpdated(theme);

        // ASSERT
        var expectedEvent = new ThemeChangedEvent(
            theme.Id,
            theme.Title,
            theme.Summary,
            theme.Slug);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenOnReleaseSlugChanged_ThenEventPublished(bool isPublicationArchived)
    {
        // ARRANGE
        var releaseId = Guid.NewGuid();
        var newReleaseSlug = "new-release-slug";
        var publicationId = Guid.NewGuid();
        var publicationSlug = "publication-slug";

        var sut = GetSut();

        // ACT
        await sut.OnReleaseSlugChanged(
            releaseId,
            newReleaseSlug,
            publicationId,
            publicationSlug,
            isPublicationArchived);

        // ASSERT
        var expectedEvent = new ReleaseSlugChangedEvent(
            releaseId,
            newReleaseSlug,
            publicationId,
            publicationSlug,
            isPublicationArchived);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnPublicationArchived_ThenEventPublished()
    {
        // ARRANGE
        var publicationId = Guid.NewGuid();
        const string publicationSlug = "publication-slug";
        var supersededByPublicationId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationArchived(
            publicationId,
            publicationSlug,
            supersededByPublicationId);

        // ASSERT
        var expectedEvent = new PublicationArchivedEvent(
            publicationId,
            publicationSlug,
            supersededByPublicationId);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnPublicationChanged_ThenEventPublished()
    {
        // ARRANGE
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Summary = "This is the publication summary",
            Slug = "publication-slug"
        };

        var sut = GetSut();

        // ACT
        await sut.OnPublicationChanged(publication);

        // ASSERT
        var expectedEvent = new PublicationChangedEvent(
            publication.Id,
            publication.Slug,
            publication.Title,
            publication.Summary,
            isPublicationArchived: false);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }
    
    [Fact]
    public async Task GivenPublicationIsArchived_WhenOnPublicationChanged_ThenEventPublishedWithIsPublicationArchivedTrue()
    {
        // ARRANGE
        var supersedingPublication = new Publication
        {
            Id = Guid.NewGuid(), 
            LatestPublishedReleaseVersionId = Guid.NewGuid()
        };
        
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Summary = "This is the publication summary",
            Slug = "publication-slug",
            SupersededById = supersedingPublication.Id,
            SupersededBy = supersedingPublication
        };

        var sut = GetSut();

        // ACT
        await sut.OnPublicationChanged(publication);

        // ASSERT
        var expectedEvent = new PublicationChangedEvent(
            publication.Id,
            publication.Slug,
            publication.Title,
            publication.Summary,
            isPublicationArchived: true);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenOnPublicationDeleted_ThenEventPublished(bool hasPublishedReleaseVersion)
    {
        // ARRANGE
        var publicationId = Guid.NewGuid();
        const string publicationSlug = "publication-slug";
        var latestPublishedReleaseVersion = hasPublishedReleaseVersion
            ? new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid()
            }
            : null;

        var sut = GetSut();

        // ACT
        await sut.OnPublicationDeleted(
            publicationId,
            publicationSlug,
            latestPublishedReleaseId: latestPublishedReleaseVersion?.ReleaseId,
            latestPublishedReleaseVersionId: latestPublishedReleaseVersion?.Id);

        // ASSERT
        var expectedEvent = new PublicationDeletedEvent(
            publicationId,
            publicationSlug,
            latestPublishedReleaseVersion?.ReleaseId,
            latestPublishedReleaseVersion?.Id);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnPublicationLatestPublishedReleaseReordered_ThenEventPublished()
    {
        // ARRANGE
        var latestPublishedReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            ReleaseId = Guid.NewGuid()
        };

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Slug = "publication-slug",
            LatestPublishedReleaseVersionId = latestPublishedReleaseVersion.Id,
            LatestPublishedReleaseVersion = latestPublishedReleaseVersion
        };
        var previousLatestPublishedReleaseId = Guid.NewGuid();
        var previousLatestPublishedReleaseVersionId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationLatestPublishedReleaseReordered(
            publication,
            previousLatestPublishedReleaseId,
            previousLatestPublishedReleaseVersionId);

        // ASSERT
        var expectedEvent = new PublicationLatestPublishedReleaseReorderedEvent(
            publication.Id,
            publication.Title,
            publication.Slug,
            publication.LatestPublishedReleaseVersion.ReleaseId,
            publication.LatestPublishedReleaseVersionId.Value,
            previousLatestPublishedReleaseId,
            previousLatestPublishedReleaseVersionId,
            isPublicationArchived: false);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task GivenPublicationIsArchived_WhenOnPublicationLatestPublishedReleaseReordered_ThenEventPublishedWithIsPublicationArchivedTrue()
    {
        // ARRANGE
        var supersedingPublication = new Publication
        {
            Id = Guid.NewGuid(), LatestPublishedReleaseVersionId = Guid.NewGuid()
        };
        
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Slug = "publication-slug",
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
            LatestPublishedReleaseVersion = new ReleaseVersion{ ReleaseId = Guid.NewGuid() },
            SupersededById = supersedingPublication.Id,
            SupersededBy = supersedingPublication,
        };
        var previousLatestPublishedReleaseId = Guid.NewGuid();
        var previousLatestPublishedReleaseVersionId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationLatestPublishedReleaseReordered(
            publication,
            previousLatestPublishedReleaseId,
            previousLatestPublishedReleaseVersionId);

        // ASSERT
        var expectedEvent = new PublicationLatestPublishedReleaseReorderedEvent(
            publication.Id,
            publication.Title,
            publication.Slug,
            publication.LatestPublishedReleaseVersion.ReleaseId,
            publication.LatestPublishedReleaseVersionId.Value,
            previousLatestPublishedReleaseId,
            previousLatestPublishedReleaseVersionId,
            isPublicationArchived: true);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnPublicationRestored_ThenEventPublished()
    {
        // ARRANGE
        var publicationId = Guid.NewGuid();
        const string publicationSlug = "publication-slug";
        var previousSupersededByPublicationId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationRestored(
            publicationId: publicationId,
            publicationSlug,
            previousSupersededByPublicationId: previousSupersededByPublicationId);

        // ASSERT
        var expectedEvent = new PublicationRestoredEvent(
            publicationId,
            publicationSlug,
            previousSupersededByPublicationId);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task
        GivenPublicationLatestPublishedReleaseVersionIdIsNull_WhenOnPublicationLatestPublishedReleaseReordered_ThenNoEventPublished()
    {
        // ARRANGE
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Summary = "This is the publication summary",
            Slug = "publication-slug",
            LatestPublishedReleaseVersionId = null
        };
        var previousLatestPublishedReleaseId = Guid.NewGuid();
        var previousLatestPublishedReleaseVersionId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationLatestPublishedReleaseReordered(
            publication,
            previousLatestPublishedReleaseId,
            previousLatestPublishedReleaseVersionId);

        // ASSERT
        _eventRaiserMockBuilder.Assert.NoEventRaised();
    }
}
