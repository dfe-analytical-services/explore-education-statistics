using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;

public class ConfiguredEventGridClientFactoryTests
{
    private readonly EventGridClientFactoryMockBuilder _eventGridClientFactory = new();
    private readonly EventGridOptionsBuilder _eventGridOptions = new();
    private readonly ILogger<ConfiguredEventGridClientFactory> _logger = NullLogger<ConfiguredEventGridClientFactory>.Instance;

    private ConfiguredEventGridClientFactory GetSut() => new(
            _eventGridClientFactory.Build(),
            _eventGridOptions.Build(),
            _logger);

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public void GivenNoConfigDefined_WhenClientRequested_ThenReturnsNull()
    {
        // ARRANGE
        _eventGridOptions.WhereNoTopicConfigDefined();
        var sut = GetSut();

        // ACT
        var wasSuccessful = sut.TryCreateClient("MyTopicOptionsKey", out var client);
            
        // ASSERT
        Assert.False(wasSuccessful);
        Assert.Null(client);
    }
    
    [Fact]
    public void GivenConfigDefined_WhenClientRequested_ThenCreatesClientUsingEndpointAndAccessKey()
    {
        // ARRANGE
        var topicConfigKey = "MyTopicOptionsKey";
        var topicEndpoint = "http://my.topic.endpoint";
        var accessKey = "top secret topic access key";
        _eventGridOptions.AddTopicConfig(topicConfigKey, topicEndpoint, accessKey);

        var sut = GetSut();

        // ACT
        var wasSuccessful = sut.TryCreateClient(topicConfigKey, out var client);
            
        // ASSERT
        Assert.True(wasSuccessful);
        _eventGridClientFactory.Assert.ClientRequested(
            actualEndpoint => actualEndpoint == topicEndpoint,
            actualAccessKey => actualAccessKey == accessKey);        
    }
    
    [Fact]
    public void GivenConfigDefinedButBlank_WhenClientRequested_ThenReturnsNull()
    {
        // ARRANGE
        var topicConfigKey = "MyTopicOptionsKey";
        var topicEndpoint = "";
        var accessKey = "";
        _eventGridOptions.AddTopicConfig(topicConfigKey, topicEndpoint, accessKey);

        var sut = GetSut();

        // ACT
        var wasSuccessful = sut.TryCreateClient(topicConfigKey, out var client);
            
        // ASSERT
        Assert.False(wasSuccessful);
        Assert.Null(client);        
    }
    
    [Fact]
    public void GivenConfigDefinedButInvalid_WhenClientRequested_ThenReturnsNull()
    {
        // ARRANGE
        var topicConfigKey = "MyTopicOptionsKey";
        var topicEndpoint = "invalid topic endpoint";
        var accessKey = "invalid access key";
        _eventGridOptions.AddTopicConfig(topicConfigKey, topicEndpoint, accessKey);
        _eventGridClientFactory.WhereCreatingClientFails();
        
        var sut = GetSut();

        // ACT
        var wasSuccessful = sut.TryCreateClient(topicConfigKey, out var client);
            
        // ASSERT
        Assert.False(wasSuccessful);
        Assert.Null(client);        
    }
}
