using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Publisher.Events;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/// <summary>
/// Published events specific to the Publisher
/// </summary>
/// <param name="eventGridClientFactory"></param>
public class PublisherEventRaiserService(
    IConfiguredEventGridClientFactory eventGridClientFactory)
    : IPublisherEventRaiserService
{
    /// <summary>
    /// On Release Version Published
    /// </summary>
    /// <param name="publishedReleaseVersionInfos">information about the one or more release versions that have been published</param>
    public async Task RaiseReleaseVersionPublishedEvents(
        IList<PublishingCompletionService.PublishedReleaseVersionInfo> publishedReleaseVersionInfos)
    {
        if (!eventGridClientFactory.TryCreateClient(
                ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                out var client))
        {
            return;
        }

        var releaseVersionPublishedEvents = 
            publishedReleaseVersionInfos.Select(info => 
                new ReleaseVersionPublishedEventDto(
                        info.ReleaseVersionId,
                        new ReleaseVersionPublishedEventDto.EventPayload
                        {
                            ReleaseId = info.ReleaseId,
                            ReleaseSlug = info.ReleaseSlug,
                            PublicationId = info.PublicationId,
                            PublicationSlug = info.PublicationSlug,
                            PublicationLatestPublishedReleaseVersionId = info.PublicationLatestPublishedReleaseVersionId,
                        })
                .ToEventGridEvent());

        foreach (var evnt in releaseVersionPublishedEvents)
        {
            await client.SendEventAsync(evnt);
        }
    }
}
