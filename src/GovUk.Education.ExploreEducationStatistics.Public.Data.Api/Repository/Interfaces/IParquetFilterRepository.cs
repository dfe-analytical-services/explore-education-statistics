using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetFilterRepository
{
    Task<IList<ParquetFilterOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<IList<ParquetFilterOption>> ListOptions(
        DataSetVersion dataSetVersion,
        IEnumerable<string> publicIds,
        CancellationToken cancellationToken = default
    );

    Task<IList<IdPublicIdPair>> ListOptionPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<Dictionary<string, string>> GetFilterColumnsById(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    );
}
