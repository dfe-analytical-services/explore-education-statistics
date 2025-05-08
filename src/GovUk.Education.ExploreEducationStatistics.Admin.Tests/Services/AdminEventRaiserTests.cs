#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Events;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

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
        var expectedEvent = new ThemeChangedEvent(theme);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnReleaseSlugChanged_ThenEventPublished()
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
            publicationSlug);

        // ASSERT
        var expectedEvent = new ReleaseSlugChangedEvent(
            releaseId,
            newReleaseSlug,
            publicationId,
            publicationSlug);
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
        var expectedEvent = new PublicationChangedEvent(publication);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }

    [Fact]
    public async Task WhenOnPublicationLatestPublishedReleaseReordered_ThenEventPublished()
    {
        // ARRANGE
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Summary = "This is the publication summary",
            Slug = "publication-slug",
            LatestPublishedReleaseVersionId = Guid.NewGuid()
        };
        var previousLatestPublishedReleaseVersionId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationLatestPublishedReleaseReordered(
            publication,
            previousLatestPublishedReleaseVersionId);

        // ASSERT
        var expectedEvent = new PublicationLatestPublishedReleaseReorderedEvent(
            publication,
            previousLatestPublishedReleaseVersionId);
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
        var previousLatestPublishedReleaseVersionId = Guid.NewGuid();

        var sut = GetSut();

        // ACT
        await sut.OnPublicationLatestPublishedReleaseReordered(
            publication,
            previousLatestPublishedReleaseVersionId);

        // ASSERT
        _eventRaiserMockBuilder.Assert.NoEventRaised();
    }
}
