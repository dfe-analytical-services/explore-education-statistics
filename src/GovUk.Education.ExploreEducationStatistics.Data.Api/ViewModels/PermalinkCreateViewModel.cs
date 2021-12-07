#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public record PermalinkCreateViewModel
    {
        public TableBuilderConfiguration Configuration { get; init; } = new();

        public ObservationQueryContext Query { get; init; } = new();
    }
}
