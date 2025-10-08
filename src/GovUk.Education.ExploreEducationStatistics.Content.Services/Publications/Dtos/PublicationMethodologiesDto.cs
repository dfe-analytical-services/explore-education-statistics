using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationMethodologiesDto
{
    public required PublicationMethodologyDto[] Methodologies { get; init; }

    public PublicationExternalMethodologyDto? ExternalMethodology { get; init; }
}

public record PublicationExternalMethodologyDto
{
    public required string Title { get; init; }

    public required string Url { get; init; }

    public static PublicationExternalMethodologyDto? FromExternalMethodology(ExternalMethodology externalMethodology) =>
        new() { Title = externalMethodology.Title, Url = externalMethodology.Url };
}

public record PublicationMethodologyDto
{
    public required Guid MethodologyId { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public static PublicationMethodologyDto FromMethodology(Methodology methodology) =>
        methodology.LatestPublishedVersion != null
            ? new PublicationMethodologyDto
            {
                MethodologyId = methodology.Id,
                Title = methodology.LatestPublishedVersion.Title,
                Slug = methodology.LatestPublishedVersion.Slug,
            }
            : throw new InvalidOperationException("Methodology must have a published version");
}
