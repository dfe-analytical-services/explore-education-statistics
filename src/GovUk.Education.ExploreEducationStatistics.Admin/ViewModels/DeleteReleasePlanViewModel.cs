#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DeleteReleasePlanViewModel
{
    public IReadOnlyList<IdTitleViewModel> ScheduledMethodologies { get; init; } = [];

    public IReadOnlyList<DeleteApiDataSetVersionPlanViewModel> ApiDataSetVersions { get; init; } = [];

    public bool Valid => ApiDataSetVersions.All(dsv => dsv.Valid);
}
