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
}

