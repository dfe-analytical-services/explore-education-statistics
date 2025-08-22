namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

public interface IDataProcessorClient
{
    public Task Import(Guid importId, CancellationToken cancellationToken = default);

    public Task CancelImport(Guid importId, CancellationToken cancellationToken = default);
}
