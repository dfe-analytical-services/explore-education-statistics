using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnPublicationLatestPublishedReleaseVersionChanged;

public class OnPublicationLatestPublishedReleaseReorderedFunctionTests
{
    private OnPublicationLatestPublishedReleaseReorderedFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Bools), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadContainsSlugAndPublicationIsNotArchived_ThenRefreshReleaseDocumentsReturned(
        bool? isPublicationArchived
    )
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto
        {
            Slug = "this-is-a-publication-slug",
            IsPublicationArchived = isPublicationArchived,
        };

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.NotNull(response);
        var actual = Assert.Single(response.RefreshReleaseDocuments);
        Assert.NotNull(actual);
        Assert.Equal(payload.Slug, actual.PublicationSlug);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GivenEvent_OnlyWhenPayloadContainsPreviousReleaseId_ThenRemoveReleaseDocumentReturned(
        bool hasPreviousReleaseId
    )
    {
        // ARRANGE
        Guid? previousReleaseId = hasPreviousReleaseId ? Guid.NewGuid() : null;

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(
                new PublicationLatestPublishedReleaseReorderedEventDto
                {
                    Slug = "this-is-a-publication-slug",
                    PreviousReleaseId = previousReleaseId,
                }
            )
            .Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.NotNull(response);

        RemoveReleaseDocumentDto[] expected = hasPreviousReleaseId
            ? [new RemoveReleaseDocumentDto { ReleaseId = previousReleaseId! }]
            : [];
        Assert.Equal(expected, response.RemoveReleaseDocuments);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainSlug_ThenNothingIsReturned(string? blankSlug)
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto { Slug = blankSlug };

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Equal(OnPublicationLatestPublishedReleaseReorderedOutput.Empty, response);
    }

    [Fact]
    public async Task GivenEvent_WhenPublicationIsArchived_ThenNothingIsReturned()
    {
        // ARRANGE
        var payload = new PublicationLatestPublishedReleaseReorderedEventDto
        {
            Slug = "this-is-a-publication-slug",
            IsPublicationArchived = true,
        };

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnPublicationLatestPublishedReleaseReordered(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Equal(OnPublicationLatestPublishedReleaseReorderedOutput.Empty, response);
    }
}
