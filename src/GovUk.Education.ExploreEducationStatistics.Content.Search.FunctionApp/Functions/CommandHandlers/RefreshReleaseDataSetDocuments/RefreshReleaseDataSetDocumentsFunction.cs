using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments;

public class RefreshReleaseDataSetDocumentsFunction(
    ILogger<RefreshReleaseDataSetDocumentsFunction> logger,
    IDataSetDocumentCreator dataSetDocumentCreator,
    IDataSetDocumentRemover dataSetDocumentRemover,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RefreshReleaseDataSetDocuments))]
    [QueueOutput("%DataSetDocumentsChangedQueueName%")]
    public async Task<DataSetDocumentsChangedMessageDto[]> RefreshReleaseDataSetDocuments(
        [QueueTrigger("%RefreshReleaseDataSetDocumentsQueueName%")] RefreshReleaseDataSetDocumentsMessageDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RefreshReleaseDataSetDocuments, message, context.CancellationToken);

    private async Task<DataSetDocumentsChangedMessageDto[]> RefreshReleaseDataSetDocuments(
        RefreshReleaseDataSetDocumentsMessageDto message,
        CancellationToken cancellationToken
    )
    {
        if (
            message.PublicationId.IsBlank()
            || string.IsNullOrWhiteSpace(message.PublicationSlug)
            || message.ReleaseId.IsBlank()
            || string.IsNullOrWhiteSpace(message.ReleaseSlug)
        )
        {
            return [];
        }

        var created = await dataSetDocumentCreator.CreateReleaseDataSetDocuments(
            new CreateReleaseDataSetDocumentsRequest
            {
                PublicationId = message.PublicationId.Value,
                PublicationSlug = message.PublicationSlug,
                ReleaseId = message.ReleaseId.Value,
                ReleaseSlug = message.ReleaseSlug,
            },
            cancellationToken
        );

        // Remove the data set documents which the release should no longer have
        var removed = await dataSetDocumentRemover.RemoveDataSetDocuments(
            new RemoveDataSetDocumentsRequest
            {
                PublicationId = message.PublicationId.Value,
                ReleaseId = message.ReleaseId.Value,
                Except = created.Keys,
            },
            cancellationToken
        );

        var uploadedBlobNames = created.Keys.Select(key => key.BlobName).ToList();

        logger.LogInformation(
            """Refreshed data set documents for release "{ReleaseSlug}" of publication "{PublicationSlug}". Uploaded: {@UploadedBlobNames}. Removed: {@RemovedBlobNames}""",
            message.ReleaseSlug,
            message.PublicationSlug,
            uploadedBlobNames,
            removed.RemovedBlobNames
        );

        if (uploadedBlobNames.Count == 0 && removed.RemovedBlobNames.Count == 0)
        {
            return [];
        }

        return
        [
            new DataSetDocumentsChangedMessageDto
            {
                PublicationId = message.PublicationId,
                ReleaseId = message.ReleaseId,
            },
        ];
    }
}
