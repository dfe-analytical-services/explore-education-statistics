using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSummaryViewModel
{
    public Guid Id { get; }

    public string Title { get; }

    public string Slug { get; }

    public string YearTitle { get; }

    public string CoverageTitle { get; }

    public DateTime? Published { get; }

    public string ReleaseName { get; }

    public PartialDate NextReleaseDate { get; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; }

    public bool LatestRelease { get; }

    public PublicationSummaryViewModel? Publication { get; }

    public ReleaseSummaryViewModel(ReleaseCacheViewModel release, PublicationCacheViewModel publication)
    {
        Id = release.Id;
        Title = release.Title;
        Slug = release.Slug;
        YearTitle = release.YearTitle;
        CoverageTitle = release.CoverageTitle;
        Published = release.Published;
        ReleaseName = release.ReleaseName;
        NextReleaseDate = release.NextReleaseDate;
        Type = release.Type;
        LatestRelease = Id == publication.LatestReleaseId;
        Publication = new PublicationSummaryViewModel(publication);
    }

    public ReleaseSummaryViewModel(ReleaseVersion releaseVersion, bool latestPublishedRelease)
    {
        Id = releaseVersion.Id;
        Title = releaseVersion.Title;
        Slug = releaseVersion.Slug;
        YearTitle = releaseVersion.YearTitle;
        CoverageTitle = releaseVersion.TimePeriodCoverage.GetEnumLabel();
        Published = releaseVersion.Published;
        ReleaseName = releaseVersion.ReleaseName;
        NextReleaseDate = releaseVersion.NextReleaseDate;
        Type = releaseVersion.Type;
        LatestRelease = latestPublishedRelease;
    }
}
