﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseSlugChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnReleaseSlugChanged;

public class OnReleaseSlugChangedFunctionTests
{
    private OnReleaseSlugChangedFunction GetSut() => new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsPublicationSlug_ThenRefreshSearchableDocumentMessageDtoReturned()
    {
        // ARRANGE
        var payload = new ReleaseSlugChangedEventDto
        {
            PublicationSlug = "this-is-a-publication-slug",
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseSlugChangedEvent(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        var actual = Assert.Single(response);
        Assert.NotNull(actual);
        Assert.Equal(payload.PublicationSlug, actual.PublicationSlug);
    }
    
    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainPublicationSlug_ThenNothingIsReturned(string? blankPublicationSlug)
    {
        // ARRANGE
        var payload = new ReleaseSlugChangedEventDto
        {
            PublicationSlug = blankPublicationSlug,
        };

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload)
            .Build();

        var sut = GetSut();
        
        // ACT
        var response = await sut.OnReleaseSlugChangedEvent(eventGridEvent, new FunctionContextMockBuilder().Build());
        
        // ASSERT
        Assert.Empty(response);
    }
}
