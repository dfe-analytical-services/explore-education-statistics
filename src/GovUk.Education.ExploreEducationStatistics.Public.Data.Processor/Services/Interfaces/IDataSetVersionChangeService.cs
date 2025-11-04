namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionChangeService
{
    Task CreateChanges(Guid nextDataSetVersionId, CancellationToken cancellationToken = default);
}
