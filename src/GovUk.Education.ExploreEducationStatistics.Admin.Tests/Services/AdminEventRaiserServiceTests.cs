using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Events;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class AdminEventRaiserServiceTests(ITestOutputHelper output)
{
    private readonly ConfiguredEventGridClientFactoryMockBuilder _eventGridClientFactoryMockBuilder = new();
    
    private IAdminEventRaiserService GetSut() => 
        new AdminEventRaiserService(_eventGridClientFactoryMockBuilder.Build());

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenNoTopicConfiguration_WhenEventRaised_ThenNothingHappens()
    {
        // ARRANGE
        _eventGridClientFactoryMockBuilder.WhereNoTopicConfigFound();
        var sut = GetSut();
        var theme = new ThemeBuilder().Build();
        
        // ACT
        await sut.OnThemeUpdated(theme);
        
        // ASSERT
        _eventGridClientFactoryMockBuilder
            .Client
            .Assert.NoEventsWerePublished();
    }
    
    [Fact]
    public async Task GivenTopicConfigured_WhenOnThemeUpdated_ThenEventPublished()
    {
        // ARRANGE
        var sut = GetSut();
        var theme = new ThemeBuilder().Build();
        
        // ACT
        await sut.OnThemeUpdated(theme);
        
        // ASSERT
        var actualEvent = Assert.Single(
            _eventGridClientFactoryMockBuilder
                .Client
                .Assert.EventsPublished);

        var expectedEvent = new ThemeChangedEventDto(theme);
        var expectedPayload = new ThemeChangedEventDto.EventPayload
        {
            Title = theme.Title,
            Summary = theme.Summary,
            Slug = theme.Slug
        };
        // The theme id should be the subject
        Assert.Equal(expectedEvent.Subject, theme.Id.ToString());

        // Ensure the payload is correct
        var actualPayload = actualEvent.Data.ToObjectFromJson<ThemeChangedEventDto.EventPayload>();
        Assert.Equal(expectedPayload, actualPayload);

        // Output the EventGridEvent - useful for adding to a function app processing queue in Microsoft Azure Storage Explorer 
        PrintJson(actualEvent);
    }
    
    private void Print(string s) => output.WriteLine(s);
    private void PrintJson<T>(T o)
    {
        var s= JsonSerializer.Serialize(o);
        Print(s);
    }
}
