using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;

public record OnPublicationLatestPublishedReleaseReorderedOutput
{
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public RefreshReleaseDocumentMessageDto[] RefreshReleaseDocuments { get; init; } = [];

    [QueueOutput("%RemoveReleaseDocumentQueueName%")]
    public RemoveReleaseDocumentDto[] RemoveReleaseDocuments { get; init; } = [];

    public static OnPublicationLatestPublishedReleaseReorderedOutput Empty => new();
}
