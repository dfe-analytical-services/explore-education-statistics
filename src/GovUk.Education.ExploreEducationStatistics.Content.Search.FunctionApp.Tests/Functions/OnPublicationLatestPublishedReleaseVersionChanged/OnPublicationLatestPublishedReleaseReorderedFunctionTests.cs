using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
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
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GivenEvent_OnlyWhenPayloadContainsPreviousReleaseVersionId_ThenRemoveSearchableDocumentReturned(bool hasPreviousReleaseVersionId)
    {
        // ARRANGE
        Guid? previousReleaseVersionId = hasPreviousReleaseVersionId 
                                            ? Guid.NewGuid() 
                                            : null;

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(new PublicationLatestPublishedReleaseReorderedEventDto
            {
                Slug = "this-is-a-publication-slug",
                PreviousReleaseVersionId = previousReleaseVersionId
            })
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent, 
            new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.NotNull(response);

        RemoveSearchableDocumentDto[] expected = hasPreviousReleaseVersionId
            ? [new RemoveSearchableDocumentDto{ ReleaseId = previousReleaseVersionId! }]
            : [];
        Assert.Equal(expected, response.RemoveSearchableDocuments);
    }
    
    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
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
        Assert.Equal(OnPublicationLatestPublishedReleaseReorderedOutput.Empty, response);
    }
}
