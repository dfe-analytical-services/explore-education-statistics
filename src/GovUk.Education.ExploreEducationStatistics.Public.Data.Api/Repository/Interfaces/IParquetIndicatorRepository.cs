using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetIndicatorRepository
{
    Task<ISet<string>> ListIds(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
