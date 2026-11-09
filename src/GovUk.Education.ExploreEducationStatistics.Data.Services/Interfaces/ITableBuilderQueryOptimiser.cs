using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ITableBuilderQueryOptimiser
{
    Task<bool> IsCroppingRequired(IStorageDataSet dataSet, FullTableQuery query);

    Task<FullTableQuery> CropQuery(IStorageDataSet dataSet, FullTableQuery query, CancellationToken cancellationToken);
}
