using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record LegacyReleaseViewModel
{
    public LegacyReleaseViewModel()
    {
    }

    public LegacyReleaseViewModel(LegacyRelease legacyRelease)
    {
        Id = legacyRelease.Id;
        Description = legacyRelease.Description;
        Url = legacyRelease.Url;

        var releaseSeriesItem = legacyRelease.Publication.ReleaseSeriesView.Find(ro => ro.ReleaseId == legacyRelease.Id)
            ?? throw new KeyNotFoundException($"No matching ReleaseSeriesItem found for {nameof(LegacyRelease)} \"{legacyRelease.Description}\"");

        Order = releaseSeriesItem.Order;
    }

    public Guid Id { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int Order { get; init; }
}
