namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveDataSetDocuments.Dto;

/// <summary>
/// Removes the data set documents of a publication. If a release id is specified, only those of that release are
/// removed. If a file id is specified as well, only the document of that data file is removed.
/// </summary>
public record RemoveDataSetDocumentsMessageDto
{
    public Guid? PublicationId { get; init; }
    public Guid? ReleaseId { get; init; }
    public Guid? FileId { get; init; }
}
