using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string YearTitle { get; init; }

    public required string CoverageTitle { get; init; }

    public DateTime? Published { get; init; }

    public PartialDate? NextReleaseDate { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleaseType Type { get; init; }

    public required bool LatestRelease { get; init; }

    public PublicationSummaryViewModel? Publication { get; init; }

    [JsonConstructor]
    public ReleaseSummaryViewModel()
    {
    }

    [SetsRequiredMembers]
    public ReleaseSummaryViewModel(ReleaseCacheViewModel release, PublicationCacheViewModel publication)
    {
        Id = release.Id;
        Title = release.Title;
        Slug = release.Slug;
        YearTitle = release.YearTitle;
        CoverageTitle = release.CoverageTitle;
        Published = release.Published;
        NextReleaseDate = release.NextReleaseDate;
        Type = release.Type;
        LatestRelease = Id == publication.LatestReleaseId;
        Publication = new PublicationSummaryViewModel(publication);
    }

    [SetsRequiredMembers]
    public ReleaseSummaryViewModel(ReleaseVersion releaseVersion, bool latestPublishedRelease)
    {
        Id = releaseVersion.Id;
        Title = releaseVersion.Title;
        Slug = releaseVersion.Slug;
        YearTitle = releaseVersion.YearTitle;
        CoverageTitle = releaseVersion.TimePeriodCoverage.GetEnumLabel();
        Published = releaseVersion.Published;
        NextReleaseDate = releaseVersion.NextReleaseDate;
        Type = releaseVersion.Type;
        LatestRelease = latestPublishedRelease;
    }
}
