using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IIndicatorRepository
{
    Task CreateIndicatorMetas(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default);

    Task CreateIndicatorMetaTable(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
