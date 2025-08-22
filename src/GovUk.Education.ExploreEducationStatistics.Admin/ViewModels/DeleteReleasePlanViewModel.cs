#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DeleteReleasePlanViewModel
{
    public IReadOnlyList<IdTitleViewModel> ScheduledMethodologies { get; init; } = [];
}
