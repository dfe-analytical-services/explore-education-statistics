using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments;

public class CreateSearchableReleaseDocumentInAzureStorageFunction(
    ILogger<CreateSearchableReleaseDocumentInAzureStorageFunction> logger,
    ISearchableDocumentCreator searchableDocumentCreator)
{
    [Function("CreateSearchableReleaseDocument")]
    [QueueOutput("%SearchableDocumentCreatedQueueName%")]
    public async Task<SearchDocumentCreatedMessage> CreateSearchableReleaseDocumentInAzureStorage(
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")]
        ReleasePublishedMessage message,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{FunctionName} triggered: {Request}", context.FunctionDefinition.Name, message);

        var request = new CreatePublicationLatestReleaseSearchableDocumentRequest{ PublicationSlug = message.PublicationSlug };
        var response = await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(request, cancellationToken);
        var searchDocumentCreatedMessage = new SearchDocumentCreatedMessage
        {
            PublicationSlug = response.PublicationSlug,
            BlobName = response.BlobName,
            ReleaseVersionId = response.ReleaseVersionId
        };

        logger.LogInformation("{FunctionName} completed. {Response}", context.FunctionDefinition.Name, searchDocumentCreatedMessage);
        return searchDocumentCreatedMessage;
    }
}
