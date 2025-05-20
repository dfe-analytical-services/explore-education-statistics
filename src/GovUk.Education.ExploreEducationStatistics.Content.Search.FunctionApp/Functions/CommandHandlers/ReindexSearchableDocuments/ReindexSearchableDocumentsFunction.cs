using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexSearchableDocuments;

public class ReindexSearchableDocumentsFunction(
    ILogger<ReindexSearchableDocumentsFunction> logger,
    ISearchIndexerClient searchIndexerClient,
    ICommandHandler commandHandler)
{
    [Function(nameof(ReindexSearchableDocuments))]
    public async Task ReindexSearchableDocuments(
        [QueueTrigger("%SearchableDocumentCreatedQueueName%")]
        SearchableDocumentCreatedMessageDto _,
        FunctionContext context) =>
        await commandHandler.Handle(
            searchIndexerClient.RunIndexer, 
            context.CancellationToken);
}
