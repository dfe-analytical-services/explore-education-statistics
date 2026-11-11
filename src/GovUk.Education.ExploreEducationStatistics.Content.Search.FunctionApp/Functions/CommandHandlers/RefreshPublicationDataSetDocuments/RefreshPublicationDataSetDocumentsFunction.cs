using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments;

public class RefreshPublicationDataSetDocumentsFunction(
    ILogger<RefreshPublicationDataSetDocumentsFunction> logger,
    IDataSetDocumentCreator dataSetDocumentCreator,
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
        if (string.IsNullOrWhiteSpace(message.PublicationSlug))
        {
            return [];
        }

        var response = await dataSetDocumentCreator.RefreshPublicationDataSetDocuments(
            new RefreshPublicationDataSetDocumentsRequest { PublicationSlug = message.PublicationSlug },
            cancellationToken
        );

        logger.LogInformation(
            """Refreshed data set documents for publication "{PublicationSlug}". Response: {@response}""",
            message.PublicationSlug,
            response
        );

        if (response.UploadedBlobNames.Count == 0 && response.RemovedBlobNames.Count == 0)
        {
            return [];
        }

        return [new DataSetDocumentsChangedMessageDto()];
    }
}
