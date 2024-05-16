using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface ILocationRepository
{
    Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        IDuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);

    Task CreateLocationMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default);

    Task CreateLocationMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
