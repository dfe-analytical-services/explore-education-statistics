#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PairedIndicatorsViewModel(List<PairedIndicatorViewModel> PairedIndicators)
{
    public static PairedIndicatorsViewModel Create(List<PairedIndicatorEntry>? pairedIndicators) =>
        new(
            pairedIndicators
                ?.Select(pair => new PairedIndicatorViewModel(
                    ParentIndicatorId: pair.ParentIndicatorId,
                    ChildIndicatorId: pair.ChildIndicatorId
                ))
                .ToList()
                ?? []
        );
}

public record PairedIndicatorViewModel(Guid ParentIndicatorId, Guid ChildIndicatorId);
