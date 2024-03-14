using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationCacheViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public Guid LatestReleaseId { get; init; }

    public bool IsSuperseded { get; init; }

    public PublicationSupersededByViewModel? SupersededBy { get; set; } = new();

    public List<ReleaseVersionTitleViewModel> Releases { get; init; } = new();

    public List<LegacyReleaseViewModel> LegacyReleases { get; init; } = new();

    public TopicViewModel Topic { get; init; } = null!;

    public ContactViewModel Contact { get; init; } = null!;

    public ExternalMethodologyViewModel? ExternalMethodology { get; init; }

    public List<ReleaseSeriesItem> ReleaseSeriesView { get; set; } = new();
}
