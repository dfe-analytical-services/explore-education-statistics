using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationTitleDto
{
    public required Guid PublicationId { get; set; }

    public required string Title { get; set; }

    public static PublicationTitleDto FromPublication(Publication publication) =>
        new() { PublicationId = publication.Id, Title = publication.Title };
}
