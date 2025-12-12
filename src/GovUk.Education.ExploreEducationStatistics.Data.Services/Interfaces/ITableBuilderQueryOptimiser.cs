using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ITableBuilderQueryOptimiser
{
    Task<bool> IsCroppingRequired(FullTableQuery query);

    Task<FullTableQuery> CropQuery(FullTableQuery query, CancellationToken cancellationToken);
}
