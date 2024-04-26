using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataSetVersionImportRepository
{
    Task UpdateStage(Guid dataSetVersionId,
        DataSetVersionImportStage stage,
        CancellationToken cancellationToken = default);
}
