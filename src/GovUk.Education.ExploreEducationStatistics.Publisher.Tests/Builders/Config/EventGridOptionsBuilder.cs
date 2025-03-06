using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Config;

public class EventGridOptionsBuilder
{
    private readonly List<EventTopicOptions> _eventTopicOptions = new();
    public IOptions<EventGridOptions> Build() =>
        Microsoft.Extensions.Options.Options.Create(
            new EventGridOptions
            {
                EventTopics = _eventTopicOptions.ToArray()
            });

    public EventGridOptionsBuilder AddTopicConfig(string topicKey, string topicEndpoint, string? topicAccessKey = null)
    {
        _eventTopicOptions.Add(new EventTopicOptions{ Key = topicKey, TopicEndpoint = topicEndpoint, TopicAccessKey = topicAccessKey});
        return this;
    }
}
