using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationArchived;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationArchived.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.OnPublicationArchived;

public class OnPublicationArchivedFunctionTests
{
    private OnPublicationArchivedFunction GetSut() => new(
        new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsSlug_ReturnsExpectedDto()
    {
        var payload = new PublicationChangedEventDto { Slug = "publication-slug" };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut().OnPublicationArchived(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(payload.Slug, actual.PublicationSlug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GivenEvent_WhenPayloadDoesNotContainSlug_ThenNothingIsReturned(string? blankSlug)
    {
        var payload = new PublicationArchivedEventDto { Slug = blankSlug };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut().OnPublicationArchived(eventGridEvent, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }
}
