#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationSitemapItemDto
{
    public required string Slug { get; init; }

    public DateTime? LastModified { get; init; }

    public ReleaseSitemapItemDto[]? Releases { get; init; }
}
