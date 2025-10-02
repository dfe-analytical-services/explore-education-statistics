using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetLocationRepository
{
    Task<IList<ParquetLocationOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<IList<ParquetLocationOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<IDataSetQueryLocation> locations,
        CancellationToken cancellationToken = default
    );

    Task<IList<IdPublicIdPair>> ListOptionPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<ISet<GeographicLevel>> ListLevels(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    );
}
