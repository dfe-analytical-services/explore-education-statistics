namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetMetaService
{
    Task CreateDataSetVersionMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
