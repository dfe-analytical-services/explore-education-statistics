using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnPublicationLatestPublishedReleaseReordered;

public class OnPublicationLatestPublishedReleaseReorderedFunctionTests
{
    private OnPublicationLatestPublishedReleaseReorderedFunction GetSut() => new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsSlug_ThenRefreshSearchableDocumentMessageDtoReturned()
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto
        {
            Slug = "this-is-a-publication-slug",
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
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
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto
        {
            Slug = blankSlug,
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent, 
            new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Empty(response);
    }
}
