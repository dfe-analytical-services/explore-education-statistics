using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments;

public class RefreshPublicationDataSetDocumentsFunction(
    ILogger<RefreshPublicationDataSetDocumentsFunction> logger,
    IDataSetDocumentCreator dataSetDocumentCreator,
    IDataSetDocumentRemover dataSetDocumentRemover,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RefreshPublicationDataSetDocuments))]
    [QueueOutput("%DataSetDocumentsChangedQueueName%")]
    public async Task<DataSetDocumentsChangedMessageDto[]> RefreshPublicationDataSetDocuments(
        [QueueTrigger("%RefreshPublicationDataSetDocumentsQueueName%")]
            RefreshPublicationDataSetDocumentsMessageDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RefreshPublicationDataSetDocuments, message, context.CancellationToken);

    private async Task<DataSetDocumentsChangedMessageDto[]> RefreshPublicationDataSetDocuments(
        RefreshPublicationDataSetDocumentsMessageDto message,
        CancellationToken cancellationToken
    )
    {
        if (message.PublicationId.IsBlank() || string.IsNullOrWhiteSpace(message.PublicationSlug))
        {
            return [];
        }

        var created = await dataSetDocumentCreator.CreatePublicationDataSetDocuments(
            new CreatePublicationDataSetDocumentsRequest
            {
                PublicationId = message.PublicationId.Value,
                PublicationSlug = message.PublicationSlug,
            },
            cancellationToken
        );

        // Remove the data set documents which the publication should no longer have
        var removed = await dataSetDocumentRemover.RemoveDataSetDocuments(
            new RemoveDataSetDocumentsRequest { PublicationId = message.PublicationId.Value, Except = created.Keys },
            cancellationToken
        );

        var uploadedBlobNames = created.Keys.Select(key => key.BlobName).ToList();

        logger.LogInformation(
            """Refreshed data set documents for publication "{PublicationSlug}". Uploaded: {@UploadedBlobNames}. Removed: {@RemovedBlobNames}""",
            message.PublicationSlug,
            uploadedBlobNames,
            removed.RemovedBlobNames
        );

        if (uploadedBlobNames.Count == 0 && removed.RemovedBlobNames.Count == 0)
        {
            return [];
        }

        return [new DataSetDocumentsChangedMessageDto { PublicationId = message.PublicationId }];
    }
}
