namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IParquetService
{
    Task WriteDataFiles(Guid dataSetVersionId, CancellationToken cancellationToken = default);
}
