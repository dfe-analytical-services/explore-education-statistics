using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IGeographicLevelMetaRepository
{
    Task<GeographicLevelMeta> ReadGeographicLevelMeta(
        IDuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    );

    Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        IDuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    );
}
