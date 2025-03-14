using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationCacheViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public Guid LatestReleaseId { get; init; }

    public bool IsSuperseded { get; init; }

    public PublicationSupersededByViewModel? SupersededBy { get; init; } = new();

    public List<ReleaseTitleViewModel> Releases { get; init; } = [];

    public List<ReleaseSeriesItemViewModel> ReleaseSeries { get; init; } = [];

    public ThemeViewModel Theme { get; init; } = null!;

    public ContactViewModel Contact { get; init; } = null!;

    public ExternalMethodologyViewModel? ExternalMethodology { get; init; }
}
