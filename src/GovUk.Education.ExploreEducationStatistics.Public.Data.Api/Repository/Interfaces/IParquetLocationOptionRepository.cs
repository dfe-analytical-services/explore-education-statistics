using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetLocationOptionRepository
{
    Task<IEnumerable<ParquetLocationOption>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ParquetLocationOption>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<DataSetQueryLocation> locations,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<IdPublicIdPair>> ListPublicIds(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);
}
