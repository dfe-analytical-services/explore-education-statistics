using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataSetVersionImportRepository
{
    Task SetCompleted(Guid dataSetVersionId,
        DateTimeOffset completed,
        CancellationToken cancellationToken = default);

    Task SetStage(Guid dataSetVersionId,
        DataSetVersionImportStage stage,
        CancellationToken cancellationToken = default);
}
