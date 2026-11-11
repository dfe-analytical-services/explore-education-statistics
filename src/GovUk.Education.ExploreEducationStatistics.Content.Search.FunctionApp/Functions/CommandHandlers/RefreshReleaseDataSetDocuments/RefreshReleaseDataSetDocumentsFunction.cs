using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDataSetDocuments;

public class RefreshReleaseDataSetDocumentsFunction(
    ILogger<RefreshReleaseDataSetDocumentsFunction> logger,
    IDataSetDocumentCreator dataSetDocumentCreator,
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

        var response = await dataSetDocumentCreator.RefreshReleaseDataSetDocuments(
            new RefreshReleaseDataSetDocumentsRequest
            {
                PublicationId = message.PublicationId.Value,
                PublicationSlug = message.PublicationSlug,
                ReleaseId = message.ReleaseId.Value,
                ReleaseSlug = message.ReleaseSlug,
            },
            cancellationToken
        );

        logger.LogInformation(
            """Refreshed data set documents for release "{ReleaseSlug}" of publication "{PublicationSlug}". Response: {@response}""",
            message.ReleaseSlug,
            message.PublicationSlug,
            response
        );

        if (response.UploadedBlobNames.Count == 0 && response.RemovedBlobNames.Count == 0)
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
