using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

public class QueryState
{
    public List<ErrorViewModel> Errors { get; } = [];

    public List<WarningViewModel> Warnings { get; } = [];
}
