using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Events;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Config;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class EventRaiserServiceTests(ITestOutputHelper output)
{
    private readonly EventGridOptionsBuilder _eventGridOptionsBuilder = new();
    private readonly UnitTestOutputLoggerBuilder<EventRaiserService> _loggerBuilder = new();

    private IEventRaiserService GetSut() => new EventRaiserService(
        _eventGridOptionsBuilder.Build(),
        _loggerBuilder.Build(output));

    public class BasicTests(ITestOutputHelper output) : EventRaiserServiceTests(output)
    {
        [Fact]
        public void Can_instantiate_sut() => Assert.NotNull(GetSut());
    }

    public class IntegrationTests(ITestOutputHelper output) : EventRaiserServiceTests(output)
    {
        // Define a topic and access key to run this integration test
        private const string TopicName = "-- add test topic name here --";
        private const string TopicAccessKey = "-- add topic access key here --";
        private const string TopicRegion = "-- add topic region here e.g. uksouth-1 --";

        private string TestTopicEndpoint => $"https://{TopicName}.{TopicRegion}.eventgrid.azure.net/api/events";

        [Fact(Skip = "Integration test to test the event raiser service")]
        public async Task WhenTopicDefinedAndReleaseVersionPublished_ThenEventsRaised()
        {
            // ARRANGE
            _eventGridOptionsBuilder
                .AddTopicConfig(
                    ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                    TestTopicEndpoint,
                    TopicAccessKey);

            var sut = GetSut();

            var publishedReleaseVersionInfo = new PublishingCompletionService.PublishedReleaseVersionInfo
            {
                PublicationId = Guid.Parse("11111111-0000-0000-0000-000000000000"),
                PublicationSlug = "test-publication-slug",
                ReleaseId = Guid.Parse("11111111-2222-0000-0000-000000000000"),
                ReleaseSlug = "test-release-slug",
                ReleaseVersionId = Guid.Parse("11111111-2222-3333-0000-000000000000"),
                PublicationLatestReleaseVersionId = Guid.Parse("11111111-2222-4444-0000-000000000000")
            };

            // ACT
            await sut.RaiseReleaseVersionPublishedEvents([publishedReleaseVersionInfo]);

            // ASSERT
            // Check unit test output for logs
            // Check for the event in https://portal.azure.com/
        }
    }

    public class ConfigTests(ITestOutputHelper output) : EventRaiserServiceTests(output)
    {
        [Fact]
        public void Ensure_config_is_read_in()
        {
            var configJson = """
                             {
                               "EventGrid":
                               {
                                 "EventTopics":
                                 [
                                    {
                                        "Key":"Event topic options key 1",
                                        "TopicEndpoint":"event topic endpoint 1",
                                        "TopicAccessKey":"event topic access key"
                                    },
                                    {
                                        "Key":"Event topic options key 2",
                                        "TopicEndpoint":"event topic endpoint 2"
                                    }
                                 ]
                               }
                             }
                             """;

            var config = new ConfigurationBuilder().AddJsonStream(configJson.ToStream()).Build();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfiguration>(config)
                .Configure<EventGridOptions>(config.GetSection(EventGridOptions.Section))
                .BuildServiceProvider();

            var actual = serviceProvider.GetRequiredService<IOptions<EventGridOptions>>();
            Assert.NotEmpty(actual.Value.EventTopics);
            var eventTopic1 = Assert.Single(actual.Value.EventTopics.Where(t => t.Key == "Event topic options key 1"));
            Assert.Equal("Event topic options key 1", eventTopic1.Key);
            Assert.Equal("event topic endpoint 1", eventTopic1.TopicEndpoint);
            Assert.Equal("event topic access key", eventTopic1.TopicAccessKey);

            var eventTopic2 = Assert.Single(actual.Value.EventTopics.Where(t => t.Key == "Event topic options key 2"));
            Assert.Equal("Event topic options key 2", eventTopic2.Key);
            Assert.Equal("event topic endpoint 2", eventTopic2.TopicEndpoint);
            Assert.Null(eventTopic2.TopicAccessKey);
        }
    }
}
