using System;
using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.EventGrid;

public class EventGridPublisherClientFactory : IEventGridPublisherClientFactory
{
    public IEventGridPublisherClient CreateClient(string topicEndpoint, string? topicAccessKey) =>
        new EventGridPublisherClientWrapper(
            topicAccessKey is null
                ? new EventGridPublisherClient(new Uri(topicEndpoint), new DefaultAzureCredential())
                : new EventGridPublisherClient(new Uri(topicEndpoint), new AzureKeyCredential(topicAccessKey)));
}
