using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDatas;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnPublicationLatestPublishedReleaseVersionChanged;

public class OnPublicationLatestPublishedReleaseReorderedFunctionTests
{
    private OnPublicationLatestPublishedReleaseReorderedFunction GetSut() => new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsSlug_ThenRefreshSearchableDocumentsReturned()
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
        Assert.NotNull(response);
        var actual = Assert.Single(response.RefreshSearchableDocuments);
        Assert.NotNull(actual);
        Assert.Equal(payload.Slug, actual.PublicationSlug);
        Assert.Empty(response.RemoveSearchableDocuments);
    }
    
    [Fact]
    public async Task GivenEvent_WhenPayloadContainsPreviousReleaseVersionId_ThenRemoveSearchableDocumentReturned()
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto
        {
            Slug = "this-is-a-publication-slug",
            PreviousReleaseVersionId = Guid.NewGuid()
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
        Assert.NotNull(response);
        var actual = Assert.Single(response.RemoveSearchableDocuments);
        Assert.NotNull(actual);
        Assert.Equal(payload.PreviousReleaseVersionId, actual.ReleaseId);
    }
    
    [Theory]
    [MemberData(nameof(Empty.StringValues), MemberType = typeof(Empty))]
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
        Assert.NotNull(response);
        Assert.Empty(response.RefreshSearchableDocuments);
        Assert.Empty(response.RemoveSearchableDocuments);
    }
}
