using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public interface IReleaseEntryDto;

public abstract record ReleaseEntryBaseDto : IReleaseEntryDto
{
    public required string Title { get; init; }
}

public record ReleaseEntryDto : ReleaseEntryBaseDto
{
    public required Guid ReleaseId { get; init; }

    public required bool IsLatestRelease { get; init; }

    public required string? Label { get; init; }

    public required DateTime LastUpdated { get; init; }

    public required DateTime Published { get; init; }

    public required string Slug { get; init; }

    public required string CoverageTitle { get; init; }

    public required string YearTitle { get; init; }

    public static ReleaseEntryDto FromRelease(
        Release release,
        bool isLatestRelease,
        DateTime lastUpdated,
        DateTime published) =>
        new()
        {
            ReleaseId = release.Id,
            IsLatestRelease = isLatestRelease,
            Label = release.Label,
            LastUpdated = lastUpdated,
            Published = published,
            Slug = release.Slug,
            Title = release.Title,
            CoverageTitle = release.TimePeriodCoverage.GetEnumLabel(),
            YearTitle = release.YearTitle
        };
}

public record LegacyReleaseEntryDto : ReleaseEntryBaseDto
{
    public required string Url { get; init; }

    public static LegacyReleaseEntryDto FromLegacyReleaseEntry(LegacyReleaseEntry legacyRelease) =>
        new()
        {
            Title = legacyRelease.Title,
            Url = legacyRelease.Url
        };
}
