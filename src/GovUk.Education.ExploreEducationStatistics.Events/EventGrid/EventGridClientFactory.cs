using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

/// <summary>
/// Create an instance of IEventGridClient for a specified topic and optional access key.
/// If no access key is supplied, the default Azure credentials are used e.g. role based security
/// </summary>
/// <param name="clientLoggerFactory">Factory to create loggers for clients</param>
public class EventGridClientFactory(Func<ILogger<SafeEventGridClient>> clientLoggerFactory)
    : IEventGridClientFactory
{
    public IEventGridClient CreateClient(string topicEndpoint, string? topicAccessKey) =>
        new SafeEventGridClient(
            clientLoggerFactory(),
            topicAccessKey is null
                ? new EventGridPublisherClient(new Uri(topicEndpoint), new DefaultAzureCredential())
                : new EventGridPublisherClient(new Uri(topicEndpoint), new AzureKeyCredential(topicAccessKey)));
}
