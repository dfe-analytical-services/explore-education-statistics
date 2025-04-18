using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

/// <summary>
/// The ConfiguredEventGridClientFactory obtains the configuration for an IEventGridClient using a specified
/// configuration key. The actual configuration, such as the Topic endpoint URL, is obtained from
/// <see cref="EventGridOptions"/>. This information is used to create a configured client via the IEventGridClientFactory.
/// </summary>
public class ConfiguredEventGridClientFactory(
    IEventGridClientFactory eventGridClientFactory,
    IOptions<EventGridOptions> eventGridOptions,
    ILogger<ConfiguredEventGridClientFactory> logger)
    : IConfiguredEventGridClientFactory
{
    /// <summary>
    /// Attempt to create an Event Grid Publisher using the configuration key specified.
    /// If no key is found then a warning will be logged.
    /// </summary>
    /// <param name="configKey">The configuration section loaded into <see cref="EventGridOptions"/> has a collection of EventTopics configurations. This key specifies which one to use.</param>
    /// <param name="client">If the configuration for the specified key is found then a configured client will be returned.</param>
    /// <returns>True if the configuration was found and a client was successfully created.</returns>
    public bool TryCreateClient(string configKey, [NotNullWhen(true)]out IEventGridClient? client)
    {
        var options = eventGridOptions.Value.EventTopics.SingleOrDefault(opt => opt.Key == configKey);
        if (options is null || string.IsNullOrEmpty(options.TopicEndpoint))
        {
            logger.LogWarning(
                "No Event Topic is configured for key {EventTopicOptionsKey}. No events will be published.", 
                configKey);
            
            client = null;
            return false;
        }

        try
        {
            client = eventGridClientFactory.CreateClient(options.TopicEndpoint, options.TopicAccessKey);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred whilst trying to create Event Grid Client with topic endpoint {TopicEndpoint}. Please check configuration for Topic Options Key {EventTopicOptionsKey}", 
                options.TopicEndpoint,
                configKey);
            
            client = null;
            return false;
        }
        return true;
    }
}
