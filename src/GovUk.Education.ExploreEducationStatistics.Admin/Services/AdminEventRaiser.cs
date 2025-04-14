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
/// <param name="eventGridClientFactory"></param>
public class AdminEventRaiser(IConfiguredEventGridClientFactory eventGridClientFactory) : IAdminEventRaiser
{
    /// <summary>
    /// On Theme Updated
    /// </summary>
    public async Task OnThemeUpdated(Theme theme)
    {
        if (!eventGridClientFactory.TryCreateClient(
                ThemeChangedEvent.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var evnt = new ThemeChangedEvent(theme);
        
        await client.SendEventAsync(evnt.ToEventGridEvent());
    }

    /// <summary>
    /// On Release Slug changed
    /// </summary>
    public async Task OnReleaseSlugChanged(Guid releaseId, string newReleaseSlug, Guid publicationId, string publicationSlug)
    {
        if (!eventGridClientFactory.TryCreateClient(
                ReleaseSlugChangedEvent.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var evnt = new ReleaseSlugChangedEvent(releaseId, newReleaseSlug, publicationId, publicationSlug);
        await client.SendEventAsync(evnt.ToEventGridEvent());
    }

    /// <summary>
    /// On Publication Changed
    /// </summary>
    public async Task OnPublicationChanged(Publication publication)
    {
        if (!eventGridClientFactory.TryCreateClient(
                PublicationChangedEvent.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var evnt = new PublicationChangedEvent(publication);
        
        await client.SendEventAsync(evnt.ToEventGridEvent());
    }
    
    /// <summary>
    /// On Publication Latest Published Release Reordered.
    /// It is assumed that the publication LatestPublishedReleaseVersionId has a value assigned to it.
    /// If it is null, then no event will be raised.
    /// </summary>
    public async Task OnPublicationLatestPublishedReleaseReordered(Publication publication, Guid previousLatestPublishedReleaseVersionId)
    {
        if (!eventGridClientFactory.TryCreateClient(
                PublicationLatestPublishedReleaseReorderedEvent.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        // Should the publication not have a latest published release version for some reason, do nothing.
        if (publication.LatestPublishedReleaseVersionId is null)
        {
            return;
        }

        var evnt = new PublicationLatestPublishedReleaseReorderedEvent(publication, previousLatestPublishedReleaseVersionId);
        
        await client.SendEventAsync(evnt.ToEventGridEvent());
    }
}

