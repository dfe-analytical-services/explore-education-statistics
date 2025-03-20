using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.ReindexSearchableDocuments;

public class ReindexSearchableDocumentsFunction(
    ILogger<ReindexSearchableDocumentsFunction> logger,
    ISearchIndexClient searchIndexClient)
{
    [Function("ReindexSearchableDocuments")]
    public async Task ReindexSearchableDocuments(
        [QueueTrigger("%SearchableDocumentCreatedQueueName%")]
        SearchableDocumentCreatedMessageDto messageDto,
        FunctionContext context)
    {
        logger.LogInformation("{FunctionName} triggered: {@Request}", context.FunctionDefinition.Name, messageDto);

        await searchIndexClient.RunIndexer(context.CancellationToken);
        
        logger.LogInformation("{FunctionName} completed.", context.FunctionDefinition.Name);
    }
}
