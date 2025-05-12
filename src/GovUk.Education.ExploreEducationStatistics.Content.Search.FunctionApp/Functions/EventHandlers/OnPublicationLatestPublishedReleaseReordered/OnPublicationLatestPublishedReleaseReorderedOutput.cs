using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered;

public record OnPublicationLatestPublishedReleaseReorderedOutput
{
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public RefreshSearchableDocumentMessageDto[] RefreshSearchableDocuments { get; init; } = [];

    [QueueOutput("%RemoveSearchableDocumentQueueName%")]
    public RemoveSearchableDocumentDto[] RemoveSearchableDocuments { get; init; } = [];
    
    public static OnPublicationLatestPublishedReleaseReorderedOutput Empty => new();
}
