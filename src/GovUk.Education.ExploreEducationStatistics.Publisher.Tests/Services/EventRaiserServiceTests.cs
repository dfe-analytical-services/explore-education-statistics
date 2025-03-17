using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Events;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Config;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services.EventGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class EventRaiserServiceTests(ITestOutputHelper output)
{
    private readonly EventGridPublisherClientFactoryBuilder _eventGridPublisherClientBuilder = new();
    private readonly EventGridOptionsBuilder _eventGridOptionsBuilder = new();
    private readonly UnitTestOutputLoggerBuilder<EventRaiserService> _loggerBuilder = new();

    private IEventRaiserService GetSut() => new EventRaiserService(
        _eventGridPublisherClientBuilder.Build(),
        _eventGridOptionsBuilder.Build(),
        _loggerBuilder.Build(output));

    public class BasicTests(ITestOutputHelper output) : EventRaiserServiceTests(output)
    {
        [Fact]
        public void Can_instantiate_sut() => Assert.NotNull(GetSut());
    }

    public class RaiseReleaseVersionPublishedEvents(ITestOutputHelper output) : EventRaiserServiceTests(output)
    {
        [Fact]
        public async Task GivenNotConfigured_WhenPublishedReleaseVersionInfoSpecified_ThenNoEventRaised()
        {
            // ARRANGE
            _eventGridOptionsBuilder.WhereNoTopicConfigFor(ReleaseVersionPublishedEventDto.EventTopicOptionsKey);
            
            var sut = GetSut();
            var publishedReleaseVersionInfo = new PublishingCompletionService.PublishedReleaseVersionInfo();
            
            // ACT
            await sut.RaiseReleaseVersionPublishedEvents([publishedReleaseVersionInfo]);

            // ASSERT
            _eventGridPublisherClientBuilder
                .Client
                .Assert.NoEventsWerePublished();
        }
        
        [Fact]
        public async Task GivenConfigured_WhenOnePublishedReleaseVersionInfoSpecified_ThenEventRaised()
        {
            // ARRANGE
            _eventGridOptionsBuilder.AddTopicConfig(
                ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                "http://test.topic.endpoint/",
                topicAccessKey: null);
            
            var sut = GetSut();
            var info = new PublishingCompletionService.PublishedReleaseVersionInfo
            {
                PublicationId = Guid.Parse("11111111-0000-0000-0000-000000000000"),
                PublicationSlug = "test-publication-slug",
                ReleaseId = Guid.Parse("11111111-2222-0000-0000-000000000000"),
                ReleaseSlug = "test-release-slug",
                ReleaseVersionId = Guid.Parse("11111111-2222-3333-0000-000000000000"),
                PublicationLatestReleaseVersionId = Guid.Parse("11111111-2222-4444-0000-000000000000")
            };
            
            // ACT
            await sut.RaiseReleaseVersionPublishedEvents([info]);

            // ASSERT
            var eventGridEvent = Assert.Single(_eventGridPublisherClientBuilder.Client.Assert.EventsPublished);
            Assert.NotNull(eventGridEvent);
            Assert.Equal(info.ReleaseVersionId.ToString(), eventGridEvent.Subject);
            Assert.Equal(ReleaseVersionPublishedEventDto.EventType, eventGridEvent.EventType);
            Assert.Equal(ReleaseVersionPublishedEventDto.DataVersion, eventGridEvent.DataVersion);
            
            var jsonPayload = eventGridEvent.Data.ToString();
            var payload = JsonSerializer.Deserialize<ReleaseVersionPublishedEventDto.EventPayload>(jsonPayload);
            Assert.NotNull(payload);
            Assert.Equal(info.ReleaseId, payload.ReleaseId);
            Assert.Equal(info.ReleaseSlug, payload.ReleaseSlug);
            Assert.Equal(info.PublicationId, payload.PublicationId);
            Assert.Equal(info.PublicationSlug, payload.PublicationSlug);
            Assert.Equal(info.PublicationLatestReleaseVersionId, payload.PublicationLatestReleaseVersionId);
        }
        
        [Theory]
        [InlineData(2)]
        [InlineData(10)]
        public async Task GivenConfigured_WhenNPublishedReleaseVersionInfoSpecified_ThenNEventRaised(int numberOfEvents)
        {
            // ARRANGE
            _eventGridOptionsBuilder.AddTopicConfig(
                ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                "http://test.topic.endpoint/",
                topicAccessKey: null);
            
            var sut = GetSut();
            var infos = Enumerable.Range(1, numberOfEvents)
                .Select(i => new PublishingCompletionService.PublishedReleaseVersionInfo
                    {
                        PublicationId = Guid.Parse($"11111111-0000-0000-0000-{i:000000000000}"),
                        PublicationSlug = "test-publication-slug",
                        ReleaseId = Guid.Parse($"11111111-2222-0000-0000-{i:000000000000}"),
                        ReleaseSlug = "test-release-slug",
                        ReleaseVersionId = Guid.Parse($"11111111-2222-3333-0000-{i:000000000000}"),
                        PublicationLatestReleaseVersionId = Guid.Parse($"11111111-2222-4444-0000-{i:000000000000}")
                    })
                .ToArray();
            
            // ACT
            await sut.RaiseReleaseVersionPublishedEvents(infos);

            // ASSERT
            Assert.Equal(numberOfEvents, _eventGridPublisherClientBuilder.Client.Assert.EventsPublished.Count());
        }
        
        [Fact]
        public async Task WhenPublishingEventReturnsErrorCode_ThenErrorLogged()
        {
            // ARRANGE
            _eventGridOptionsBuilder.AddTopicConfig(
                ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                "http://test.topic.endpoint/",
                topicAccessKey: null);

            _eventGridPublisherClientBuilder.Client.WhereResponseIs(HttpStatusCode.Unauthorized);
            
            var sut = GetSut();
            var info = new PublishingCompletionService.PublishedReleaseVersionInfo();
            
            // ACT
            await sut.RaiseReleaseVersionPublishedEvents([info]);

            // ASSERT
            _loggerBuilder.Assert.LoggedErrorContains("401");
        }
        
        [Fact]
        public async Task WhenPublishingEventThrows_ThenErrorLogged()
        {
            // ARRANGE
            _eventGridOptionsBuilder.AddTopicConfig(
                ReleaseVersionPublishedEventDto.EventTopicOptionsKey,
                "http://test.topic.endpoint/",
                topicAccessKey: null);

            _eventGridPublisherClientBuilder.Client.WhereSendEventAsyncThrows(new Exception("TEST EXCEPTION"));
            
            var sut = GetSut();
            var info = new PublishingCompletionService.PublishedReleaseVersionInfo();
            
            // ACT
            await sut.RaiseReleaseVersionPublishedEvents([info]);

            // ASSERT
            _loggerBuilder.Assert.LoggedErrorContains("TEST EXCEPTION");
        }
        
    }

    public class ServiceRegistrationTests()
    {
        [Fact]
        public void WhenResolvedFromContainer_ThenEventRaiserServiceIsReturned()
        {
            // ARRAGE
            var host = new HostBuilder().ConfigurePublisherHostBuilder().Build();
            
            // ACT
            var eventRaiserService = host.Services.GetRequiredService<IEventRaiserService>();
            
            // ASSERT
            Assert.NotNull(eventRaiserService);
        }
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
