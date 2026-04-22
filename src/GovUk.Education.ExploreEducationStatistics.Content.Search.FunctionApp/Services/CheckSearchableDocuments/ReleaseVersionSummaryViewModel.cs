using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record ReleaseSummaryViewModel
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

    private ReleaseSummaryViewModel(ReleaseSummary releaseSummary)
    {
        Id = releaseSummary.Id;
        ReleaseId = releaseSummary.ReleaseId;
        Title = releaseSummary.Title;
        Slug = releaseSummary.Slug;
        YearTitle = releaseSummary.YearTitle;
        CoverageTitle = releaseSummary.CoverageTitle;
        Published = releaseSummary.Published;
        Type = releaseSummary.Type;
        IsLatestRelease = releaseSummary.IsLatestRelease;
        PublicationId = releaseSummary.PublicationId;
        PublicationTitle = releaseSummary.PublicationTitle;
        PublicationSlug = releaseSummary.PublicationSlug;
    }

    public static ReleaseSummaryViewModel FromModel(ReleaseSummary releaseSummary) => new(releaseSummary);
}
