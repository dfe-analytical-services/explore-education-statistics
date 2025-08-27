using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationSitemapReleaseDto
{
    public required string Slug { get; init; }

    public required DateTime LastModified { get; init; }

    public static PublicationSitemapReleaseDto FromReleaseVersion(ReleaseVersion releaseVersion)
    {
        return new PublicationSitemapReleaseDto
        {
            Slug = releaseVersion.Release.Slug,
            LastModified = releaseVersion.Published ?? throw new ArgumentException("ReleaseVersion must be published")
        };
    }
}
