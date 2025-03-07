using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Publisher.Events;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class EventRaiserService : IEventRaiserService
{
    private const string ReleaseVersionPublishedEventOptionsKey = "ReleaseVersionPublishedEvent";
    private readonly ILogger<EventRaiserService> _logger;
    private readonly EventGridOptions _eventGridOptions;

    public EventRaiserService(IOptions<EventGridOptions> eventGridOptions, ILogger<EventRaiserService> logger)
    {
        _logger = logger;
        _eventGridOptions = eventGridOptions.Value;
    }
    
    public async Task RaiseReleaseVersionPublishedEvents(IList<PublishingCompletionService.PublishedReleaseVersionInfo> publishedReleaseVersionInfos)
    {
        var options = _eventGridOptions.EventOptions.SingleOrDefault(opt => opt.Key == ReleaseVersionPublishedEventOptionsKey);
        if (options is null)
        {
            _logger.LogError("No EventGridOptions found for key {EventGridOptionsKey}", ReleaseVersionPublishedEventOptionsKey);
            return;
        }

        var eventGridEvents = publishedReleaseVersionInfos
            .Select(publishedReleaseVersionInfo => 
                new EventGridEvent(options.Subject, options.EventType, ReleaseVersionPublishedEventDto.DataVersion, 
                    new ReleaseVersionPublishedEventDto
                        {
                            ReleaseVersionId = publishedReleaseVersionInfo.ReleaseVersionId,
                            ReleaseId = publishedReleaseVersionInfo.ReleaseId,
                            ReleaseSlug = publishedReleaseVersionInfo.ReleaseSlug,
                            PublicationId = publishedReleaseVersionInfo.PublicationId,
                            PublicationSlug = publishedReleaseVersionInfo.PublicationSlug,
                            PublicationLatestReleaseVersionId = publishedReleaseVersionInfo.PublicationLatestReleaseVersionId,
                        }));

        var client = new EventGridPublisherClient(new Uri(options.TopicEndpoint), new DefaultAzureCredential());

        foreach (var eventGridEvent in eventGridEvents)
        {
            await RaiseEvent(client, eventGridEvent);
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
                _logger.LogError("Error occurred whilst trying to raise event {@Event}. Response:{@SendEventResponse}", eventGridEvent, response);
                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error occurred whilst trying to raise event {@Event}. Exception:{@Exception}", eventGridEvent, e);
            return;
        }
        _logger.LogInformation("Event raised: {@Event}", eventGridEvent);
    }
}
