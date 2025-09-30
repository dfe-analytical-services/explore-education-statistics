using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationRestored;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationRestored.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnPublicationRestored;

public class OnPublicationRestoredFunctionTests
{
    private OnPublicationRestoredFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsPublicationSlug_ReturnsExpectedDto()
    {
        var payload = new PublicationRestoredEventDto { PublicationSlug = "publication-slug" };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();
        var expected = new RefreshSearchableDocumentMessageDto
        {
            PublicationSlug = payload.PublicationSlug,
        };

        var response = await GetSut()
            .OnPublicationRestored(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainSlug_ThenNothingIsReturned(
        string? blankSlug
    )
    {
        var payload = new PublicationRestoredEventDto { PublicationSlug = blankSlug };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut()
            .OnPublicationRestored(eventGridEvent, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }
}
