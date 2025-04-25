using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.ReindexSearchableDocuments;

public class ReindexSearchableDocumentsFunction(
    ILogger<ReindexSearchableDocumentsFunction> logger,
    ISearchIndexerClient searchIndexerClient)
{
    [Function(nameof(ReindexSearchableDocuments))]
    public async Task ReindexSearchableDocuments(
        [QueueTrigger("%SearchableDocumentCreatedQueueName%")]
        SearchableDocumentCreatedMessageDto messageDto,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered: {@Request}", context.FunctionDefinition.Name, messageDto);

        await searchIndexerClient.RunIndexer(context.CancellationToken);
        
        logger.LogInformation("{FunctionName} completed.", context.FunctionDefinition.Name);
    }
}
