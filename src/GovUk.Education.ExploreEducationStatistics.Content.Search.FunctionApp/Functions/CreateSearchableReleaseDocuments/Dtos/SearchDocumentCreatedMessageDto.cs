namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;

public record SearchDocumentCreatedMessageDto
{
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }

    public required Guid ReleaseId {get;init;}
    public required string ReleaseSlug { get; init; }
    
    public required Guid ReleaseVersionId { get; init; }
    public required string BlobName { get; init; }
    
    public required Guid PublicationLatestReleaseVersionId { get; init; }  
}
