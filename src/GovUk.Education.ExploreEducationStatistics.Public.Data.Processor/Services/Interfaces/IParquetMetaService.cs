namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IParquetMetaService
{
    Task CreateParquetDataSetMetaTables(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
