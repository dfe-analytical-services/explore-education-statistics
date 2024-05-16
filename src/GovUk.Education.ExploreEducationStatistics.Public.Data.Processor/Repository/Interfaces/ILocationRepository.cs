using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface ILocationRepository
{
    Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);

    Task CreateLocationMetas(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default);

    Task CreateLocationMetaTable(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
