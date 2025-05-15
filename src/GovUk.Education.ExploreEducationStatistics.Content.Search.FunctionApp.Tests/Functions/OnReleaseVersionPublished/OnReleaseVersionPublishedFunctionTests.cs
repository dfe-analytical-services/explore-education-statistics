using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnReleaseVersionPublished;

public class OnReleaseVersionPublishedFunctionTests
{
    private OnReleaseVersionPublishedFunction GetSut() => new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsPublicationSlug_ThenRefreshSearchableDocumentMessageDtoReturned()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsLatest;
        
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseVersionPublished(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.NotNull(response);
        var actual = Assert.Single(response.RefreshSearchableDocumentMessages);
        Assert.NotNull(actual);
        Assert.Equal(payload.PublicationSlug, actual.PublicationSlug);
    }
    
    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainPublicationSlug_ThenEmptyIsReturned(string? blankPublicationSlug)
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NoPublicationSlug();
        
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseVersionPublished(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Equal(OnReleaseVersionPublishedOutput.Empty, response);
    }

    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsNotTheLatest_ThenEmptyIsReturned()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsNotLatest;
        
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseVersionPublished(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Equal(OnReleaseVersionPublishedOutput.Empty, response);
    }
    
    // New release, then ensure old searchable doc is removed
    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsADifferentRelease_ThenPreviousReleaseSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsLatestButDifferentRelease;
        
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseVersionPublished(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Equal([new RemoveSearchableDocumentDto{ ReleaseId = payload.PreviousLatestReleaseId }], response.RemoveSearchableDocuments);
    }
    
    // Different release version for the same release, then no document is removed - it will be overwritten
    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsSameRelease_ThenNoSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsForSameRelease;
        
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseVersionPublished(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Empty(response.RemoveSearchableDocuments);
    }

    private static class NewReleaseVersionPublishedEvents
    {
        private static ReleaseVersionPublishedEventDto Base
        {
            get
            {
                // By default, create an event where
                // - the newly published release version is the latest
                // - the newly published release version is for the same release as the previous latest release
                var newlyPublishedLatestReleaseVersionId = Guid.NewGuid();
                var newlyPublishedLatestReleaseId = Guid.NewGuid();
                return new()
                {
                    ReleaseVersionId = newlyPublishedLatestReleaseVersionId,
                    ReleaseId = newlyPublishedLatestReleaseId,
                    ReleaseSlug = "this-is-a-release-slug",
                    PublicationId = Guid.NewGuid(),
                    PublicationSlug = "this-is-a-publication-slug",
                    PublicationLatestPublishedReleaseVersionId = newlyPublishedLatestReleaseVersionId,
                    PreviousLatestReleaseId = newlyPublishedLatestReleaseId
                };
            }
        }

        public static ReleaseVersionPublishedEventDto NoPublicationSlug() => 
            Base with
            {
                PublicationSlug = null
            };
        public static ReleaseVersionPublishedEventDto NewlyPublishedIsNotLatest => 
            Base with
            {
                PublicationLatestPublishedReleaseVersionId = Guid.NewGuid()
            };

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsLatest => Base;

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsLatestButDifferentRelease => 
            Base with
            {
                PreviousLatestReleaseId = Guid.NewGuid()
            };

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsForSameRelease => Base;
    }
}
