using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record ReleaseVersionSummaryViewModel
{
    public string Id { get; init; }
    public string ReleaseId { get; init; }
    public string Title { get; init; }
    public string Slug { get; init; }
    public string? YearTitle { get; init; }
    public string? CoverageTitle { get; init; }
    public DateTimeOffset? Published { get; init; }
    public string? Type { get; init; }
    public bool? IsLatestRelease { get; init; }

    public string? PublicationId { get; init; }
    public string? PublicationTitle { get; init; }
    public string? PublicationSlug { get; init; }

    private ReleaseVersionSummaryViewModel(ReleaseVersionSummary releaseVersionSummary)
    {
        Id = releaseVersionSummary.Id;
        ReleaseId = releaseVersionSummary.ReleaseId;
        Title = releaseVersionSummary.Title;
        Slug = releaseVersionSummary.Slug;
        YearTitle = releaseVersionSummary.YearTitle;
        CoverageTitle = releaseVersionSummary.CoverageTitle;
        Published = releaseVersionSummary.Published;
        Type = releaseVersionSummary.Type;
        IsLatestRelease = releaseVersionSummary.IsLatestRelease;
        PublicationId = releaseVersionSummary.PublicationId;
        PublicationTitle = releaseVersionSummary.PublicationTitle;
        PublicationSlug = releaseVersionSummary.PublicationSlug;
    }

    public static ReleaseVersionSummaryViewModel FromModel(ReleaseVersionSummary releaseVersionSummary) =>
        new(releaseVersionSummary);
}
