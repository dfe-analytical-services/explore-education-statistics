using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IFilterRepository
{
    Task CreateFilterMetas(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default);

    Task CreateFilterMetaTable(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
