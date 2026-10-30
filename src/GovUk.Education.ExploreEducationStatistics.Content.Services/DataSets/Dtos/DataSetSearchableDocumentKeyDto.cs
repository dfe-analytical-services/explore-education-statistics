namespace GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;

public record DataSetSearchableDocumentKeyDto
{
    public required Guid PublicationId { get; init; }

    public required Guid ReleaseId { get; init; }

    public required Guid FileId { get; init; }
}
