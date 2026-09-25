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
    private OnPublicationDeletedFunction GetSut() =>
        new(new EventGridEventHandler(new NullLogger<EventGridEventHandler>()));

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
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
            },
        };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();
        var expected = new RemoveSearchableDocumentDto
        {
            ReleaseId = payload.LatestPublishedRelease.LatestPublishedReleaseId,
        };

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenEvent_WhenPayloadContainsReleaseIds_ThenAllOfThemAreRemoved()
    {
        var latestPublishedReleaseId = Guid.NewGuid();
        var supersededReleaseId = Guid.NewGuid();
        var neverPublishedReleaseId = Guid.NewGuid();

        var payload = new PublicationDeletedEventDto
        {
            ReleaseIds = [latestPublishedReleaseId, supersededReleaseId, neverPublishedReleaseId],
            LatestPublishedRelease = new LatestPublishedReleaseInfo
            {
                LatestPublishedReleaseId = latestPublishedReleaseId,
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
            },
        };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        // The latest published Release appears in both fields, but is only removed once.
        Assert.Equal(
            [
                new RemoveSearchableDocumentDto { ReleaseId = latestPublishedReleaseId },
                new RemoveSearchableDocumentDto { ReleaseId = supersededReleaseId },
                new RemoveSearchableDocumentDto { ReleaseId = neverPublishedReleaseId },
            ],
            response
        );
    }

    [Fact]
    public async Task GivenEvent_WhenPayloadHasNoReleaseIds_ThenLatestPublishedReleaseIsStillRemoved()
    {
        // Events raised before ReleaseIds was added to the payload must still be handled.
        var payload = new PublicationDeletedEventDto
        {
            LatestPublishedRelease = new LatestPublishedReleaseInfo
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
            },
        };
        var eventGridEvent = new EventGridEventBuilder().WithPayload(payload).Build();

        var response = await GetSut().OnPublicationDeleted(eventGridEvent, new FunctionContextMockBuilder().Build());

        var actual = Assert.Single(response);
        Assert.Equal(payload.LatestPublishedRelease.LatestPublishedReleaseId, actual.ReleaseId);
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
