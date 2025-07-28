using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public Guid LatestReleaseId { get; init; }

    public bool IsSuperseded { get; init; }

    public PublicationSupersededByViewModel? SupersededBy { get; init; } = new();

    public List<ReleaseTitleViewModel> Releases { get; init; } = [];

    public List<ReleaseSeriesItemViewModel> ReleaseSeries { get; init; } = [];

    public ThemeViewModel Theme { get; init; } = null!;

    public ContactViewModel Contact { get; init; } = null!;

    public ExternalMethodologyViewModel? ExternalMethodology { get; init; }

    public List<MethodologyVersionSummaryViewModel> Methodologies { get; init; } = new();

    public PublicationViewModel()
    {
    }

    public PublicationViewModel(PublicationCacheViewModel publication,
        List<MethodologyVersionSummaryViewModel> methodologySummaries)
    {
        Id = publication.Id;
        Title = publication.Title;
        Slug = publication.Slug;
        Summary = publication.Summary;
        LatestReleaseId = publication.LatestReleaseId;
        IsSuperseded = publication.IsSuperseded;
        SupersededBy = publication.SupersededBy;
        Releases = publication.Releases;
        ReleaseSeries = publication.ReleaseSeries;
        Theme = publication.Theme;
        Contact = publication.Contact;
        ExternalMethodology = publication.ExternalMethodology;
        Methodologies = methodologySummaries;
    }
}
