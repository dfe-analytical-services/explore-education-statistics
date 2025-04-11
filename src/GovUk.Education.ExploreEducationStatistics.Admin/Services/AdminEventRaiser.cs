using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
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
    public async Task OnThemeUpdated(Theme theme) => 
        await eventRaiser.RaiseEvent(new ThemeChangedEvent(theme));

    /// <summary>
    /// On Release Slug changed
    /// </summary>
    public async Task OnReleaseSlugChanged(
        Guid releaseId,
        string newReleaseSlug,
        Guid publicationId,
        string publicationSlug) =>
        await eventRaiser.RaiseEvent(new ReleaseSlugChangedEvent(releaseId, newReleaseSlug, publicationId, publicationSlug));

    /// <summary>
    /// On Publication Changed
    /// </summary>
    public async Task OnPublicationChanged(Publication publication) =>
        await eventRaiser.RaiseEvent(new PublicationChangedEvent(publication));

    /// <summary>
    /// On Publication Latest Published Release Reordered.
    /// It is assumed that the publication LatestPublishedReleaseVersionId has a value assigned to it.
    /// If it is null, then no event will be raised.
    /// </summary>
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
}

