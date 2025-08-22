using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

public class DataProcessorClient(string connectionString) : IDataProcessorClient
{
    private readonly QueueServiceClient _queueServiceClient = new(connectionString);

    public async Task Import(Guid importId, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(ProcessorQueues.ImportsPendingQueue,
            new ImportMessage(importId),
            cancellationToken);
    }

    public async Task CancelImport(Guid importId, CancellationToken cancellationToken = default)
    {
        await _queueServiceClient.SendMessageAsJson(ProcessorQueues.ImportsCancellingQueue,
            new CancelImportMessage(importId),
            cancellationToken);
    }
}
