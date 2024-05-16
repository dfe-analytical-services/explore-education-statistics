using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataRepository
{
    Task CreateDataTable(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
