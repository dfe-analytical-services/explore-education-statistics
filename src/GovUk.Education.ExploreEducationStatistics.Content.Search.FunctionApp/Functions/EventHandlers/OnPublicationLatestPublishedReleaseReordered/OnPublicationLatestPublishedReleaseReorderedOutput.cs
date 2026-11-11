using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;

public record OnPublicationLatestPublishedReleaseReorderedOutput
{
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public RefreshReleaseDocumentMessageDto[] RefreshReleaseDocumentMessages { get; init; } = [];

    [QueueOutput("%RemoveReleaseDocumentQueueName%")]
    public RemoveReleaseDocumentMessageDto[] RemoveReleaseDocumentMessages { get; init; } = [];

    public static OnPublicationLatestPublishedReleaseReorderedOutput Empty => new();
}
