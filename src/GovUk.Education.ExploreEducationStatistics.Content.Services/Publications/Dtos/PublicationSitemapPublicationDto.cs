using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationSitemapPublicationDto
{
    public required string Slug { get; init; }

    public required DateTime? LastModified { get; init; }

    public required PublicationSitemapReleaseDto[] Releases { get; init; }

    public static PublicationSitemapPublicationDto FromPublication(
        Publication publication,
        PublicationSitemapReleaseDto[] releases) =>
        new()
        {
            Slug = publication.Slug,
            LastModified = publication.Updated,
            Releases = releases
        };
}

public record PublicationSitemapReleaseDto
{
    public required string Slug { get; init; }

    public required DateTime LastModified { get; init; }

    public static PublicationSitemapReleaseDto FromReleaseVersion(ReleaseVersion releaseVersion) =>
        new()
        {
            Slug = releaseVersion.Release.Slug,
            LastModified = releaseVersion.Published ?? throw new ArgumentException("ReleaseVersion must be published")
        };
}
