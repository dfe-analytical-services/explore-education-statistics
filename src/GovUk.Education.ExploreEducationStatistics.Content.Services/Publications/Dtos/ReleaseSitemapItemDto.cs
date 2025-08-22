namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record ReleaseSitemapItemDto
{
    public required string Slug { get; init; }

    public DateTime? LastModified { get; init; }
}
