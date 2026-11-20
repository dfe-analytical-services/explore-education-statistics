using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveDataSetDocuments;

public class RemoveDataSetDocumentsFunction(
    ILogger<RemoveDataSetDocumentsFunction> logger,
    IDataSetDocumentRemover dataSetDocumentRemover,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RemoveDataSetDocuments))]
    [QueueOutput("%DataSetDocumentsChangedQueueName%")]
    public async Task<DataSetDocumentsChangedMessageDto[]> RemoveDataSetDocuments(
        [QueueTrigger("%RemoveDataSetDocumentsQueueName%")] RemoveDataSetDocumentsMessageDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RemoveDataSetDocuments, message, context.CancellationToken);

    private async Task<DataSetDocumentsChangedMessageDto[]> RemoveDataSetDocuments(
        RemoveDataSetDocumentsMessageDto message,
        CancellationToken cancellationToken
    )
    {
        if (message.PublicationId.IsBlank())
        {
            return [];
        }

        // A blank release id with a file id would otherwise remove the documents of the whole publication
        if (message.ReleaseId.IsBlank() && message.FileId.HasValue)
        {
            logger.LogWarning(
                "Ignoring message to remove the data set document of data file {FileId} because it has no release id",
                message.FileId
            );
            return [];
        }

        var response = await dataSetDocumentRemover.RemoveDataSetDocuments(
            new RemoveDataSetDocumentsRequest
            {
                PublicationId = message.PublicationId.Value,
                ReleaseId = message.ReleaseId.IsBlank() ? null : message.ReleaseId,
                FileId = message.FileId.IsBlank() ? null : message.FileId,
            },
            cancellationToken
        );

        logger.LogInformation(
            "Removed data set documents for publication {PublicationId}, release {ReleaseId}, data file {FileId}. Response: {@response}",
            message.PublicationId,
            message.ReleaseId,
            message.FileId,
            response
        );

        if (response.RemovedBlobNames.Count == 0)
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
