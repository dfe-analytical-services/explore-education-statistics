using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RemoveSearchableDocument.Dto;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseReordered;

public class OnPublicationLatestPublishedReleaseReorderedOutput
{
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public RefreshSearchableDocumentMessageDto[] RefreshSearchableDocuments { get; init; } = [];

    [QueueOutput("%RemoveSearchableDocumentQueueName%")]
    public RemoveSearchableDocumentDto[] RemoveSearchableDocuments { get; init; } = [];
    
    public static OnPublicationLatestPublishedReleaseReorderedOutput Empty => new();
}
