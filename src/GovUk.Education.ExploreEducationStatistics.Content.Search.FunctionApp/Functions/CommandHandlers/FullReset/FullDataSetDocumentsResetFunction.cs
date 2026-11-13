using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullDataSetDocumentsResetFunction(
    IFullDataSetDocumentResetter fullDataSetDocumentResetter,
    ICommandHandler commandHandler
)
{
    /// <summary>
    /// Removes all data set documents, and queues a refresh of the data set documents of every publication which has
    /// them. The data set indexer is run as those refreshes complete.
    /// </summary>
    [Function(nameof(FullDataSetDocumentsReset))]
    public async Task<FullDataSetDocumentsResetOutput> FullDataSetDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(FullDataSetDocumentsReset))]
            HttpRequest ignored, //  The binding name _ is invalid
        FunctionContext context
    ) => await commandHandler.Handle(ResetDataSetDocuments, context.CancellationToken);

    private async Task<FullDataSetDocumentsResetOutput> ResetDataSetDocuments(CancellationToken cancellationToken)
    {
        var response = await fullDataSetDocumentResetter.PerformReset(cancellationToken);

        return new FullDataSetDocumentsResetOutput
        {
            RefreshPublicationDataSetDocumentsMessages =
            [
                .. response.Publications.Select(publication => new RefreshPublicationDataSetDocumentsMessageDto
                {
                    PublicationId = publication.PublicationId,
                    PublicationSlug = publication.PublicationSlug,
                }),
            ],
            HttpResult = new OkObjectResult(
                new FullDataSetDocumentsResetResult(
                    RemovedBlobCount: response.RemovedBlobCount,
                    PublicationCount: response.Publications.Length
                )
            ),
        };
    }
}
