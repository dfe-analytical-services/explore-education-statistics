using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PreReleaseAccessListViewModel : ReleaseSummaryViewModel
{
    public required string PreReleaseAccessList { get; init; }

    [SetsRequiredMembers]
    public PreReleaseAccessListViewModel(
        ReleaseCacheViewModel release,
        PublicationCacheViewModel publication
    )
        : base(release, publication)
    {
        PreReleaseAccessList = release.PreReleaseAccessList;
    }
}
