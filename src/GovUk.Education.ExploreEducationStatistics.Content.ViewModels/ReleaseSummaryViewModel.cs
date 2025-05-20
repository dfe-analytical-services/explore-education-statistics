using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSummaryViewModel
{
    /// <summary>
    /// The ReleaseVersion id
    /// </summary>
    public required Guid Id { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string YearTitle { get; init; }

    public required string CoverageTitle { get; init; }

    public DateTime? Published { get; init; }

    public PartialDate? NextReleaseDate { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<ReleaseType>))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public required ReleaseType Type { get; init; }

    public required bool LatestRelease { get; init; }

    public PublicationSummaryViewModel? Publication { get; init; }

    public ReleaseSummaryViewModel()
    {
    }

    [SetsRequiredMembers]
    public ReleaseSummaryViewModel(ReleaseCacheViewModel release, PublicationCacheViewModel publication)
    {
        Id = release.Id;
        ReleaseId = release.ReleaseId;
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
        ReleaseId = releaseVersion.Release.Id;
        Title = releaseVersion.Release.Title;
        Slug = releaseVersion.Release.Slug;
        YearTitle = releaseVersion.Release.YearTitle;
        CoverageTitle = releaseVersion.Release.TimePeriodCoverage.GetEnumLabel();
        Published = releaseVersion.Published;
        NextReleaseDate = releaseVersion.NextReleaseDate;
        Type = releaseVersion.Type;
        LatestRelease = latestPublishedRelease;
    }
}
