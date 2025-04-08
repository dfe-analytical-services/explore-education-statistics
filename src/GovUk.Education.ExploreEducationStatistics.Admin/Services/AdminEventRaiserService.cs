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
public class AdminEventRaiserService(IConfiguredEventGridClientFactory eventGridClientFactory) : IAdminEventRaiserService
{
    /// <summary>
    /// On Theme Updated
    /// </summary>
    public async Task OnThemeUpdated(Theme theme)
    {
        if (!eventGridClientFactory.TryCreateClient(
                ThemeChangedEventDto.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var eventDto = new ThemeChangedEventDto(theme);
        
        await client.SendEventAsync(eventDto.ToEventGridEvent());
    }

    /// <summary>
    /// On Release Slug changed
    /// </summary>
    public async Task OnReleaseSlugChanged(Guid releaseId, string newReleaseSlug, Guid publicationId, string publicationSlug)
    {
        if (!eventGridClientFactory.TryCreateClient(
                ReleaseSlugChangedEventDto.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var eventDto = new ReleaseSlugChangedEventDto(releaseId, newReleaseSlug, publicationId, publicationSlug);
        await client.SendEventAsync(eventDto.ToEventGridEvent());
    }

    /// <summary>
    /// On Publication Changed
    /// </summary>
    public async Task OnPublicationChanged(Publication publication)
    {
        if (!eventGridClientFactory.TryCreateClient(
                PublicationChangedEventDto.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var eventDto = new PublicationChangedEventDto(publication);
        
        await client.SendEventAsync(eventDto.ToEventGridEvent());
    }
    
    /// <summary>
    /// On Publication Latest Published Release Version changed.
    /// It is assumed that the publication LatestPublishedReleaseVersionId has a value assigned to it.
    /// If it is null, then no event will be raised.
    /// </summary>
    public async Task OnPublicationLatestPublishedReleaseVersionChanged(Publication publication, Guid previousLatestPublishedReleaseVersionId)
    {
        if (!eventGridClientFactory.TryCreateClient(
                PublicationLatestPublishedReleaseVersionChangedEventDto.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        // Should the publication not have a latest published release version for some reason, do nothing.
        if (publication.LatestPublishedReleaseVersionId is null)
        {
            return;
        }

        var eventDto = new PublicationLatestPublishedReleaseVersionChangedEventDto(publication, previousLatestPublishedReleaseVersionId);
        
        await client.SendEventAsync(eventDto.ToEventGridEvent());
    }
}

