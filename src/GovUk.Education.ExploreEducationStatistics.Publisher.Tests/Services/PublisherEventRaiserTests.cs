using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;
using GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders.Config;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class PublisherEventRaiserTests
{
    private readonly EventRaiserMockBuilder _eventRaiserMockBuilder = new();

    private IPublisherEventRaiser GetSut(IEventRaiser? eventRaiser = null) => 
        new PublisherEventRaiser(eventRaiser ?? _eventRaiserMockBuilder.Build());

    public class BasicTests : PublisherEventRaiserTests
    {
        [Fact]
        public void Can_instantiate_sut() => Assert.NotNull(GetSut());
    }

    public class OnPublicationArchivedTests : PublisherEventRaiserTests
    {
        [Fact]
        public async Task WhenOnPublicationArchived_ThenEventRaised()
        {
            // ARRANGE
            var publicationId = Guid.NewGuid();
            const string publicationSlug = "publication-slug";
            var supersededByPublicationId = Guid.NewGuid();

            var sut = GetSut();

            // ACT
            await sut.OnPublicationArchived(
                publicationId,
                publicationSlug,
                supersededByPublicationId);

            // ASSERT
            var expectedEvent = new PublicationArchivedEvent(
                publicationId,
                publicationSlug,
                supersededByPublicationId);
            _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
        }
    }

    public class RaiseReleaseVersionPublishedPublisherEvents : PublisherEventRaiserTests
    {
        [Fact]
        public async Task WhenOnePublishedReleaseVersion_ThenEventRaised()
        {
            // ARRANGE
            var publicationId = Guid.NewGuid();
            var sut = GetSut();
            var info = new PublishedPublicationInfo
            {
                PublicationId = publicationId,
                PublicationSlug = "test-publication-slug",
                PreviousLatestPublishedReleaseId = Guid.NewGuid(),
                PreviousLatestPublishedReleaseVersionId = Guid.NewGuid(),
                LatestPublishedReleaseId = Guid.NewGuid(),
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                PublishedReleaseVersions = [
                    new PublishedReleaseVersionInfo
                    {
                        ReleaseId = Guid.NewGuid(),
                        ReleaseSlug = "test-release-slug",
                        ReleaseVersionId = Guid.NewGuid(),
                        PublicationId = publicationId
                    }
                ]
            };

            // ACT
            await sut.OnReleaseVersionsPublished([info]);

            // ASSERT
            var expectedEvent = new ReleaseVersionPublishedEvent(
                new ReleaseVersionPublishedEvent.ReleaseVersionPublishedEventInfo
                {
                    PublicationId = info.PublicationId,
                    PublicationSlug = info.PublicationSlug,
                    ReleaseId = info.PublishedReleaseVersions[0].ReleaseId,
                    ReleaseSlug = info.PublishedReleaseVersions[0].ReleaseSlug,
                    ReleaseVersionId = info.PublishedReleaseVersions[0].ReleaseVersionId,
                    PreviousLatestPublishedReleaseId = info.PreviousLatestPublishedReleaseId,
                    PreviousLatestPublishedReleaseVersionId = info.PreviousLatestPublishedReleaseVersionId,
                    LatestPublishedReleaseId = info.LatestPublishedReleaseId,
                    LatestPublishedReleaseVersionId = info.LatestPublishedReleaseVersionId
                });
            _eventRaiserMockBuilder.Assert.EventsRaised([expectedEvent]);
        }

        [Fact]
        public async Task WhenMultiplePublishedReleaseVersions_ThenEventsRaised()
        {
            // ARRANGE
            var sut = GetSut();
            var publication1Id = Guid.NewGuid();
            var publication2Id = Guid.NewGuid();
            var infos = new[]
            {
                new PublishedPublicationInfo
                {
                    PublicationId = publication1Id,
                    PublicationSlug = "test-publication-slug-1",
                    PreviousLatestPublishedReleaseId = Guid.NewGuid(),
                    PreviousLatestPublishedReleaseVersionId = Guid.NewGuid(),
                    LatestPublishedReleaseId = Guid.NewGuid(),
                    LatestPublishedReleaseVersionId = Guid.NewGuid(),
                    PublishedReleaseVersions =
                    [
                        new PublishedReleaseVersionInfo
                        {
                            ReleaseId = Guid.NewGuid(),
                            ReleaseSlug = "test-release-slug-1",
                            ReleaseVersionId = Guid.NewGuid(),
                            PublicationId = publication1Id
                        },
                        new PublishedReleaseVersionInfo
                        {
                            ReleaseId = Guid.NewGuid(),
                            ReleaseSlug = "test-release-slug-2",
                            ReleaseVersionId = Guid.NewGuid(),
                            PublicationId = publication1Id
                        }
                    ]
                },
                new PublishedPublicationInfo
                {
                    PublicationId = publication2Id,
                    PublicationSlug = "test-publication-slug-2",
                    PreviousLatestPublishedReleaseId = Guid.NewGuid(),
                    PreviousLatestPublishedReleaseVersionId = Guid.NewGuid(),
                    LatestPublishedReleaseId = Guid.NewGuid(),
                    LatestPublishedReleaseVersionId = Guid.NewGuid(),
                    PublishedReleaseVersions =
                    [
                        new PublishedReleaseVersionInfo
                        {
                            ReleaseId = Guid.NewGuid(),
                            ReleaseSlug = "test-release-slug-3",
                            ReleaseVersionId = Guid.NewGuid(),
                            PublicationId = publication2Id
                        },
                        new PublishedReleaseVersionInfo
                        {
                            ReleaseId = Guid.NewGuid(),
                            ReleaseSlug = "test-release-slug-4",
                            ReleaseVersionId = Guid.NewGuid(),
                            PublicationId = publication2Id
                        }
                    ]
                }
            };

            // ACT
            await sut.OnReleaseVersionsPublished(infos);

            // ASSERT
            var expectedEvents = infos.SelectMany(info =>
                info.PublishedReleaseVersions.Select(version =>
                    new ReleaseVersionPublishedEvent(
                        new ReleaseVersionPublishedEvent.ReleaseVersionPublishedEventInfo
                        {
                            PublicationId = info.PublicationId,
                            PublicationSlug = info.PublicationSlug,
                            ReleaseId = version.ReleaseId,
                            ReleaseSlug = version.ReleaseSlug,
                            ReleaseVersionId = version.ReleaseVersionId,
                            PreviousLatestPublishedReleaseId = info.PreviousLatestPublishedReleaseId,
                            PreviousLatestPublishedReleaseVersionId = info.PreviousLatestPublishedReleaseVersionId,
                            LatestPublishedReleaseId = info.LatestPublishedReleaseId,
                            LatestPublishedReleaseVersionId = info.LatestPublishedReleaseVersionId
                        }))).ToList();

            _eventRaiserMockBuilder.Assert.EventsRaised(expectedEvents);
        }
    }

    public class ServiceRegistrationTests
    {
        [Fact]
        public void WhenResolvedFromContainer_ThenEventRaiserIsReturned()
        {
            // ARRANGE
            var host = new HostBuilder()
                .ConfigureTestAppConfiguration()
                .ConfigureServices()
                .Build();

            // ACT
            var eventRaiser = host.Services.GetRequiredService<IPublisherEventRaiser>();

            // ASSERT
            Assert.NotNull(eventRaiser);
        }
    }

    public class IntegrationTests(ITestOutputHelper output) : PublisherEventRaiserTests
    {
        // Define a topic and access key to run this integration test
        private const string TopicName = "-- add test topic name here --";
        private const string TopicAccessKey = "-- add topic access key here --";
        private const string TopicRegion = "-- add topic region here e.g. uksouth-1 --";

        private string TestTopicEndpoint => $"https://{TopicName}.{TopicRegion}.eventgrid.azure.net/api/events";

        [Fact(Skip = "Integration test to test the event raiser")]
        public async Task WhenTopicDefinedAndReleaseVersionPublished_ThenEventsRaised()
        {
            // ARRANGE
            var eventGridOptions = new EventGridOptionsBuilder()
                .AddTopicConfig(
                    ReleaseVersionPublishedEvent.EventTopicOptionsKey,
                    TestTopicEndpoint,
                    TopicAccessKey)
                .Build();

            var realEventGridClientFactory = new ConfiguredEventGridClientFactory(
                new EventGridClientFactory(
                    () => new UnitTestOutputLoggerBuilder<SafeEventGridClient>().Build(output)),
                eventGridOptions,
                new UnitTestOutputLoggerBuilder<ConfiguredEventGridClientFactory>().Build(output));

            var sut = GetSut(new EventRaiser(realEventGridClientFactory));

            var publicationId = Guid.NewGuid();
            var info = new PublishedPublicationInfo
            {
                PublicationId = publicationId,
                PublicationSlug = "test-publication-slug",
                PreviousLatestPublishedReleaseId = Guid.NewGuid(),
                PreviousLatestPublishedReleaseVersionId = Guid.NewGuid(),
                LatestPublishedReleaseId = Guid.NewGuid(),
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                PublishedReleaseVersions = [
                    new PublishedReleaseVersionInfo
                    {
                        ReleaseId = Guid.NewGuid(),
                        ReleaseSlug = "test-release-slug",
                        ReleaseVersionId = Guid.NewGuid(),
                        PublicationId = publicationId
                    }
                ]
            };

            // ACT
            await sut.OnReleaseVersionsPublished([info]);

            // ASSERT
            // Check unit test output for logs
            // Check for the event in https://portal.azure.com/
        }
    }

    public class ConfigTests : PublisherEventRaiserTests
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
            var eventTopic1 = Assert.Single(actual.Value.EventTopics, t => t.Key == "Event topic options key 1");
            Assert.Equal("Event topic options key 1", eventTopic1.Key);
            Assert.Equal("event topic endpoint 1", eventTopic1.TopicEndpoint);
            Assert.Equal("event topic access key", eventTopic1.TopicAccessKey);

            var eventTopic2 = Assert.Single(actual.Value.EventTopics, t => t.Key == "Event topic options key 2");
            Assert.Equal("Event topic options key 2", eventTopic2.Key);
            Assert.Equal("event topic endpoint 2", eventTopic2.TopicEndpoint);
            Assert.Null(eventTopic2.TopicAccessKey);
        }
    }
}
