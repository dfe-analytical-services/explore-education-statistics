using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public record ReleaseSummaryViewModel
{
    public string Id { get; private init; }
    public string ReleaseId { get; private init; }
    public string Title { get; private init; }
    public string Slug { get; private init; }
    public string? YearTitle { get; private init; }
    public string? CoverageTitle { get; private init; }
    public DateTimeOffset? Published { get; private init; }
    public string? Type { get; private init; }
    public bool? IsLatestRelease { get; private init; }
    
    public string? PublicationId { get; private init; }
    public string? PublicationTitle { get; private init; }
    public string? PublicationSlug { get; private init; }
    
    public ReleaseSummaryViewModel(ReleaseSummary releaseSummary)
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
    
    public static ReleaseSummaryViewModel FromModel(ReleaseSummary releaseSummary)
        => new(releaseSummary);
}
