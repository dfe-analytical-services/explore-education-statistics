using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnPublicationDeleted;

public class OnPublicationDeletedFunctionTests
{
    private OnPublicationDeletedFunction GetSut() => new(
        new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

    [Fact]
    public void CanInstantiateSut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsLatestPublishedRelease_ReturnsExpectedDto()
    {
        var payload = new PublicationDeletedEventDto
        {
            LatestPublishedRelease = new LatestPublishedReleaseInfo
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                LatestPublishedReleaseVersionId = Guid.NewGuid()
            }
        };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();
        var expected = new RemoveSearchableDocumentDto
        {
            ReleaseId = payload.LatestPublishedRelease.LatestPublishedReleaseId
        };

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenEvent_WhenPayloadDoesNotContainReleaseId_ThenNothingIsReturned()
    {
        var payload = new PublicationDeletedEventDto();
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        Assert.Empty(response);
    }
}
