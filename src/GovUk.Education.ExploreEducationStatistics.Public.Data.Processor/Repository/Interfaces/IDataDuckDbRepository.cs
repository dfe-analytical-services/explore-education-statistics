namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;

public interface IDataDuckDbRepository
{
    Task CreateDataTable(Guid dataSetVersionId, CancellationToken cancellationToken = default);
}
