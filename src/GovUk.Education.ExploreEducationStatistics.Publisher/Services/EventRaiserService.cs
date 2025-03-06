using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Publisher.Events;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EventRaiserService(IOptions<EventGridOptions> eventGridOptions, ILogger<EventRaiserService> logger)
    : IEventRaiserService
{
    private readonly EventGridOptions _eventGridOptions = eventGridOptions.Value;

    public async Task RaiseReleaseVersionPublishedEvents(IList<PublishingCompletionService.PublishedReleaseVersionInfo> publishedReleaseVersionInfos)
    {
        var options = _eventGridOptions.EventTopics.SingleOrDefault(opt => opt.Key == ReleaseVersionPublishedEventDto.EventTopicOptionsKey);
        if (options is null)
        {
            logger.LogError("No Event Topic was configured for key {EventTopicOptionsKey}", ReleaseVersionPublishedEventDto.EventTopicOptionsKey);
            return;
        }

        var releaseVersionPublishedEvents = publishedReleaseVersionInfos
            .Select(info => new ReleaseVersionPublishedEventDto(
                                    info.ReleaseVersionId,
                                    new ReleaseVersionPublishedEventDto.EventPayload
                                    {
                                        ReleaseId = info.ReleaseId,
                                        ReleaseSlug = info.ReleaseSlug,
                                        PublicationId = info.PublicationId,
                                        PublicationSlug = info.PublicationSlug,
                                        PublicationLatestReleaseVersionId = info.PublicationLatestReleaseVersionId,
                                    }));

        var client = options.TopicAccessKey is null
                ? new EventGridPublisherClient(new Uri(options.TopicEndpoint), new DefaultAzureCredential())
                : new EventGridPublisherClient(new Uri(options.TopicEndpoint), new AzureKeyCredential(options.TopicAccessKey));

        foreach (var evnt in releaseVersionPublishedEvents)
        {
            await RaiseEvent(client, evnt.ToEventGridEvent());
        }
    }

    private async Task RaiseEvent(
        EventGridPublisherClient client,
        EventGridEvent eventGridEvent)
    {
        try
        {
            var response = await client.SendEventAsync(eventGridEvent);
            if (response.IsError)
            {
                logger.LogError("Error occurred whilst trying to raise event {@Event}. Response:{@SendEventResponse}", eventGridEvent, response);
                return;
            }
        }
        catch (Exception e)
        {
            logger.LogError("Error occurred whilst trying to raise event {@Event}. Exception:{@Exception}", eventGridEvent, e);
            return;
        }
        logger.LogInformation("Event raised: {@Event}", eventGridEvent);
    }
}
