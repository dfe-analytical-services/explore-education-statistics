using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexReleaseDocuments;

public class ReindexReleaseDocumentsFunction(
    ISearchIndexerClient searchIndexerClient,
    ICommandHandler commandHandler,
    IOptions<AzureSearchOptions> azureSearchOptions
)
{
    [Function(nameof(ReindexReleaseDocuments))]
    public async Task ReindexReleaseDocuments(
        [QueueTrigger("%ReleaseDocumentCreatedQueueName%")] ReleaseDocumentCreatedMessageDto ignored, //  The binding name _ is invalid
        FunctionContext context
    ) =>
        await commandHandler.Handle(
            ct => searchIndexerClient.RunIndexer(azureSearchOptions.Value.ReleaseIndexerName, ct),
            context.CancellationToken
        );
}
