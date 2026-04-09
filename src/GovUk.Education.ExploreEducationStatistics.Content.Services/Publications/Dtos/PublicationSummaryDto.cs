using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationSummaryDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Summary { get; init; }

    public required DateTimeOffset Published { get; init; }

    public static PublicationSummaryDto FromPublication(Publication publication) =>
        publication.LatestPublishedReleaseVersion != null
            ? new PublicationSummaryDto
            {
                Id = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Summary = publication.Summary,
                Published = publication.LatestPublishedReleaseVersion.PublishedDisplayDate!.Value,
            }
            : throw new InvalidOperationException("Publication must have a latest published release version");
}
