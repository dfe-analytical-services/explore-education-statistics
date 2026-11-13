namespace GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;

public record DataSetSearchableDocumentPublicationDto
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }
}
