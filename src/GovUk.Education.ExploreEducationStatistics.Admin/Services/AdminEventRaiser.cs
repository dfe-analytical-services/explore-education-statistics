#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Events;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// Publish events specific to Admin
/// </summary>
public class AdminEventRaiser(IEventRaiser eventRaiser) : IAdminEventRaiser
{
    /// <summary>
    /// On Theme Updated
    /// </summary>
    /// <param name="theme">The theme that has been updated.</param>
    public async Task OnThemeUpdated(Theme theme) =>
        await eventRaiser.RaiseEvent(new ThemeChangedEvent(theme));

    /// <summary>
    /// On Release Slug changed
    /// </summary>
    /// <param name="releaseId">The unique identifier of the release.</param>
    /// <param name="newReleaseSlug">The new slug for the release.</param>
    /// <param name="publicationId">The unique identifier of the associated publication.</param>
    /// <param name="publicationSlug">The slug of the associated publication.</param>
    public async Task OnReleaseSlugChanged(
        Guid releaseId,
        string newReleaseSlug,
        Guid publicationId,
        string publicationSlug) =>
        await eventRaiser.RaiseEvent(new ReleaseSlugChangedEvent(releaseId,
            newReleaseSlug,
            publicationId,
            publicationSlug));

    /// <summary>
    /// Publishes an event when a publication is archived.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication that has been archived.</param>
    /// <param name="publicationSlug">The slug of the publication that has been archived.</param>
    /// <param name="supersededByPublicationId">The unique identifier of the publication that has superseded the archived publication.</param>
    public async Task OnPublicationArchived(
        Guid publicationId,
        string publicationSlug,
        Guid supersededByPublicationId) =>
        await eventRaiser.RaiseEvent(new PublicationArchivedEvent(
            publicationId,
            publicationSlug,
            supersededByPublicationId));

    /// <summary>
    /// On Publication Changed
    /// </summary>
    /// <param name="publication">The publication that has been changed.</param>
    public async Task OnPublicationChanged(Publication publication) =>
        await eventRaiser.RaiseEvent(new PublicationChangedEvent(publication));

    /// <summary>
    /// On Publication Latest Published Release Reordered.
    /// It is assumed that the publication LatestPublishedReleaseVersionId has a value assigned to it.
    /// If it is null, then no event will be raised.
    /// </summary>
    /// <param name="publication">The publication whose latest published release has been reordered.</param>
    /// <param name="previousLatestPublishedReleaseVersionId">
    /// The unique identifier of the previous latest published release version.
    /// </param>
    public async Task OnPublicationLatestPublishedReleaseReordered(
        Publication publication,
        Guid previousLatestPublishedReleaseVersionId)
    {
        // Should the publication not have a latest published release version for some reason, do nothing.
        if (publication.LatestPublishedReleaseVersionId is null)
        {
            return;
        }

        await eventRaiser.RaiseEvent(
            new PublicationLatestPublishedReleaseReorderedEvent(
                publication,
                previousLatestPublishedReleaseVersionId));
    }

    /// <summary>
    /// Publishes an event when an archived publication is restored.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the archived publication that has been restored.</param>
    /// <param name="publicationSlug">The slug of the archived publication that has been restored.</param>
    /// <param name="previousSupersededByPublicationId">The unique identifier of the publication that superseded the archived publication before it was restored.</param>
    public async Task OnPublicationRestored(
        Guid publicationId,
        string publicationSlug,
        Guid previousSupersededByPublicationId) =>
        await eventRaiser.RaiseEvent(new PublicationRestoredEvent(
            publicationId,
            publicationSlug,
            previousSupersededByPublicationId));
}
