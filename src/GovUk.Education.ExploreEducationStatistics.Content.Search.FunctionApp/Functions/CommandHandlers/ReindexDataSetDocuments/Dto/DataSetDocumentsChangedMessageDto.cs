namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;

/// <summary>
/// Sent when data set documents have been created, updated or removed, so that the data set indexer is run.
/// </summary>
public record DataSetDocumentsChangedMessageDto
{
    public Guid? PublicationId { get; init; }
    public Guid? ReleaseId { get; init; }
}
