using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetTimePeriodRepository
{
    Task<IList<ParquetTimePeriod>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<IList<ParquetTimePeriod>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<DataSetQueryTimePeriod> timePeriods,
        CancellationToken cancellationToken = default
    );
}
