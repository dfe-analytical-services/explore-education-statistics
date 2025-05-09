﻿using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders.Config;

public class EventGridOptionsBuilder
{
    private readonly List<EventTopicOptions> _eventTopicOptions = new();

    public IOptions<EventGridOptions> Build() => Options.Create(
        new EventGridOptions { EventTopics = _eventTopicOptions.ToArray() });

    public EventGridOptionsBuilder AddTopicConfig(string topicKey, string topicEndpoint, string? topicAccessKey = null)
    {
        _eventTopicOptions.Add(new EventTopicOptions{ Key = topicKey, TopicEndpoint = topicEndpoint, TopicAccessKey = topicAccessKey});
        return this;
    }

    public EventGridOptionsBuilder WhereNoTopicConfigFor(string topicKey)
    {
        _eventTopicOptions.RemoveAll(o => o.Key == topicKey);
        return this;
    }

    public EventGridOptionsBuilder WhereNoTopicConfigDefined()
    {
        _eventTopicOptions.Clear();
        return this;
    }
}
