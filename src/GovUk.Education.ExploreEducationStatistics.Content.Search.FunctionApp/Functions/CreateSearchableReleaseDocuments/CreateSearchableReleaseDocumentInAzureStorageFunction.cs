using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
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
        [QueueTrigger("%ReleaseVersionPublishedQueueName%")] ReleasePublishedMessage message,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{FunctionName} triggered: {Request}", context.FunctionDefinition.Name, message);

        var publicationSlug = message.ReleaseSlug;
        var request = new CreatePublicationLatestReleaseSearchableDocumentRequest(publicationSlug);
        var response = await searchableDocumentCreator.CreatePublicationLatestReleaseSearchableDocument(request, cancellationToken);

        logger.LogInformation("{FunctionName} completed. {@Response}", context.FunctionDefinition.Name, response);

        return response switch
        {
            CreatePublicationLatestReleaseSearchableDocumentResponse.Success msg => 
                new SearchDocumentCreatedMessage
                    {
                        PublicationSlug = publicationSlug,
                        BlobName = msg.BlobName,
                        ReleaseVersionId = msg.ReleaseVersionId
                    },
            
            CreatePublicationLatestReleaseSearchableDocumentResponse.NotFound => 
                throw new UnableToCreateSearchableDocumentException(publicationSlug, "Publication Release not found."),
            
            CreatePublicationLatestReleaseSearchableDocumentResponse.Error msg => 
                throw new UnableToCreateSearchableDocumentException(publicationSlug, msg.ErrorMessage),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
