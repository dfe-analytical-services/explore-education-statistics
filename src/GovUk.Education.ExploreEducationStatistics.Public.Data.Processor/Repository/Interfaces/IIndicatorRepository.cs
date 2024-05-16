using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IIndicatorRepository
{
    Task CreateIndicatorMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default);

    Task CreateIndicatorMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
