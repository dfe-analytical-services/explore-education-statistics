using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments;

public class ReindexDataSetDocumentsFunction(
    ISearchIndexerClient searchIndexerClient,
    ICommandHandler commandHandler,
    IOptions<AzureSearchOptions> azureSearchOptions
)
{
    [Function(nameof(ReindexDataSetDocuments))]
    public async Task ReindexDataSetDocuments(
        [QueueTrigger("%DataSetDocumentsChangedQueueName%")] DataSetDocumentsChangedMessageDto ignored, //  The binding name _ is invalid
        FunctionContext context
    ) =>
        await commandHandler.Handle(
            ct => searchIndexerClient.RunIndexer(azureSearchOptions.Value.DataSetIndexerName, ct),
            context.CancellationToken
        );
}
