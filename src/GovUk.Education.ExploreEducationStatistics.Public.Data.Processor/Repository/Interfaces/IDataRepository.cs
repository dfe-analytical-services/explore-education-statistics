using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataRepository
{
    Task CreateDataTable(
        DuckDbConnection duckDb,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
