using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemovePublicationSearchableDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnPublicationArchived;

public class OnPublicationArchivedFunctionTests
{
    private OnPublicationArchivedFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsPublicationSlug_ReturnsExpectedDto()
    {
        var payload = new PublicationArchivedEventDto { PublicationSlug = "publication-slug" };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();
        var expected = new RemovePublicationSearchableDocumentsDto
        {
            PublicationSlug = payload.PublicationSlug,
        };

        var response = await GetSut()
            .OnPublicationArchived(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TheoryDatas.Blank.Strings), MemberType = typeof(TheoryDatas.Blank))]
    public async Task GivenEvent_WhenPayloadDoesNotContainSlug_ThenNothingIsReturned(
        string? blankSlug
    )
    {
        var payload = new PublicationArchivedEventDto { PublicationSlug = blankSlug };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut()
            .OnPublicationArchived(eventGridEvent, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }
}
