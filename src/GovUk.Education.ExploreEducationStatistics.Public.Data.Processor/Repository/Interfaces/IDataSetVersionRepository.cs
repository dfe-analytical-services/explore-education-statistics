using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataSetVersionRepository
{
    Task UpdateStatus(Guid dataSetVersionId,
        DataSetVersionStatus status,
        CancellationToken cancellationToken = default);
}
