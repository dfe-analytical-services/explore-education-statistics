using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnPublicationDeleted;

public class OnPublicationDeletedFunctionTests
{
    private OnPublicationDeletedFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsReleaseIds_ReturnsExpectedDtos()
    {
        Guid[] releaseIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        var eventGridEvent = new EventGridEventBuilder()
            .WithPayload(new PublicationDeletedEventDto { ReleaseIds = releaseIds })
            .Build();

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        var expected = releaseIds
            .Select(releaseId => new RemoveSearchableDocumentDto { ReleaseId = releaseId })
            .ToArray();

        Assert.Equal(expected, response);
    }

    [Fact]
    public async Task GivenEvent_WhenPayloadDoesNotContainReleaseIds_ThenNothingIsReturned()
    {
        var eventGridEvent = new EventGridEventBuilder().WithPayload(new PublicationDeletedEventDto()).Build();

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }
}
