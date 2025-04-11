using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseVersionChanged;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseVersionChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnPublicationLatestPublishedReleaseVersionChanged;

public class OnPublicationLatestPublishedReleaseVersionChangedFunctionTests
{
    private OnPublicationLatestPublishedReleaseVersionChangedFunction GetSut() => new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsSlug_ThenRefreshSearchableDocumentMessageDtoReturned()
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseVersionChangedEventDto
        {
            Slug = "this-is-a-publication-slug",
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseVersionChanged(
            eventGridEvent, 
            new FunctionContextMockBuilder().Build());
        
        // ASSERT
        var actual = Assert.Single(response);
        Assert.NotNull(actual);
        Assert.Equal(payload.Slug, actual.PublicationSlug);
    }
    
    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    public async Task GivenEvent_WhenPayloadDoesNotContainSlug_ThenNothingIsReturned(string? blankSlug)
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseVersionChangedEventDto
        {
            Slug = blankSlug,
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseVersionChanged(
            eventGridEvent, 
            new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Empty(response);
    }
}
