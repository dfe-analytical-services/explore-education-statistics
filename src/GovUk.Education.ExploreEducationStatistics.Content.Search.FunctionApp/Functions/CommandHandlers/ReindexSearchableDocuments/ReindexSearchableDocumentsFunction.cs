using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexSearchableDocuments;

public class ReindexSearchableDocumentsFunction(
    ISearchIndexerClient searchIndexerClient,
    ICommandHandler commandHandler)
{
    [Function(nameof(ReindexSearchableDocuments))]
    public async Task ReindexSearchableDocuments(
        [QueueTrigger("%SearchableDocumentCreatedQueueName%")]
        SearchableDocumentCreatedMessageDto ignored, //  The binding name _ is invalid 
        FunctionContext context) =>
        await commandHandler.Handle(
            searchIndexerClient.RunIndexer, 
            context.CancellationToken);
}
