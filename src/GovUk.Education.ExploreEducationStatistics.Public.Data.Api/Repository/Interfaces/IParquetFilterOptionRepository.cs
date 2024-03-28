using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetFilterOptionRepository
{
    Task<IEnumerable<ParquetFilterOption>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ParquetFilterOption>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<string> publicIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<IdPublicIdPair>> ListPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);
}
