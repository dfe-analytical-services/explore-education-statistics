using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnReleaseVersionPublished;

public class OnReleaseVersionPublishedFunctionTests
{
    private OnReleaseVersionPublishedFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainPublicationSlug_ThenEmptyIsReturned(
        string? blankString
    )
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.WherePublicationSlugIs(blankString);

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Equal(OnReleaseVersionPublishedOutput.Empty, response);
    }

    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsNotTheLatest_ThenEmptyIsReturned()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsNotLatest;

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Equal(OnReleaseVersionPublishedOutput.Empty, response);
    }

    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsADifferentRelease_ThenPreviousReleaseSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsForNewLatestRelease;

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        // The searchable documents are keyed on the latest Release Id.
        // If the newly published release version is for a different release then the new searchable
        // document will be created using its new ReleaseId.
        // Therefore, the existing, previously latest searchable document needs to be removed.
        Assert.Equal(
            [
                new RemoveSearchableDocumentDto
                {
                    ReleaseId = payload.PreviousLatestPublishedReleaseId,
                },
            ],
            response.RemoveSearchableDocuments
        );
    }

    [Fact]
    public async Task GivenPublicationIsArchived_WhenPublishedReleaseVersionIsADifferentRelease_ThenNoReleaseSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsForNewLatestRelease with
        {
            IsPublicationArchived = true,
        };

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Empty(response.RemoveSearchableDocuments);
    }

    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsSameRelease_ThenNoSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsForSameRelease;

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Empty(response.RemoveSearchableDocuments);
    }

    [Fact]
    public async Task GivenEvent_WhenPublishedReleaseVersionIsTheFirstRelease_ThenNoSearchableDocumentIsRemoved()
    {
        // ARRANGE
        var payload = NewReleaseVersionPublishedEvents.NewlyPublishedIsTheFirstRelease;

        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Empty(response.RemoveSearchableDocuments);
    }

    [Theory]
    [MemberData(nameof(EventsThatRefreshSearchableDocument))]
    public async Task WhenReleaseVersionPublished_ThenSearchableDocumentIsRefreshed(
        ReleaseVersionPublishedEventDto payload
    )
    {
        // ARRANGE
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        RefreshSearchableDocumentMessageDto[] expected =
        [
            new() { PublicationSlug = payload.PublicationSlug },
        ];
        Assert.Equal(expected, response.RefreshSearchableDocumentMessages);
    }

    [Theory]
    [MemberData(nameof(EventsThatRefreshSearchableDocument))]
    public async Task GivenPublicationIsArchived_WhenReleaseVersionPublished_ThenNoSearchableDocumentIsRefreshed(
        ReleaseVersionPublishedEventDto payload
    )
    {
        // ARRANGE
        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(payload with { IsPublicationArchived = true })
            .Build();

        var sut = GetSut();

        // ACT
        var response = await sut.OnReleaseVersionPublished(
            eventGridEvent,
            new FunctionContextMockBuilder().Build()
        );

        // ASSERT
        Assert.Empty(response.RefreshSearchableDocumentMessages);
    }

    public static TheoryData<ReleaseVersionPublishedEventDto> EventsThatRefreshSearchableDocument =>
        [
            NewReleaseVersionPublishedEvents.NewlyPublishedIsForNewLatestRelease,
            NewReleaseVersionPublishedEvents.NewlyPublishedIsForSameRelease,
            NewReleaseVersionPublishedEvents.NewlyPublishedIsTheFirstRelease,
        ];

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
                    LatestPublishedReleaseVersionId = newlyPublishedLatestReleaseVersionId,
                    PreviousLatestPublishedReleaseId = newlyPublishedLatestReleaseId,
                };
            }
        }

        public static ReleaseVersionPublishedEventDto WherePublicationSlugIs(
            string? publicationSlug
        ) => Base with { PublicationSlug = publicationSlug };

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsNotLatest =>
            Base with
            {
                // Latest release version id is something other than the release version just published
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
            };

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsForNewLatestRelease =>
            Base with
            {
                // The previous release id is a different release
                PreviousLatestPublishedReleaseId = Guid.NewGuid(),
            };

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsForSameRelease => Base;

        public static ReleaseVersionPublishedEventDto NewlyPublishedIsTheFirstRelease =>
            Base with
            {
                PreviousLatestPublishedReleaseId = null,
                PreviousLatestPublishedReleaseVersionId = null,
            };
    }
}
