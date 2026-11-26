using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullReleaseDocumentsResetFunction(
    IFullReleaseDocumentResetter fullReleaseDocumentResetter,
    ICommandHandler commandHandler
)
{
    /// <summary>
    /// Removes all release documents, and queues a refresh of the release document of every live publication. The
    /// release indexer is run as those refreshes complete.
    /// </summary>
    [Function(nameof(FullReleaseDocumentsReset))]
    public async Task<FullReleaseDocumentsResetOutput> FullReleaseDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(FullReleaseDocumentsReset))]
            HttpRequest ignored, //  The binding name _ is invalid
        FunctionContext context
    ) => await commandHandler.Handle(ResetReleaseDocuments, context.CancellationToken);

    private async Task<FullReleaseDocumentsResetOutput> ResetReleaseDocuments(CancellationToken cancellationToken)
    {
        var response = await fullReleaseDocumentResetter.PerformReset(cancellationToken);

        return new FullReleaseDocumentsResetOutput
        {
            RefreshReleaseDocumentMessages =
            [
                .. response.Publications.Select(publication => new RefreshReleaseDocumentMessageDto
                {
                    PublicationSlug = publication.PublicationSlug,
                }),
            ],
            HttpResult = new OkObjectResult(
                new FullReleaseDocumentsResetResult(
                    RemovedBlobCount: response.RemovedBlobCount,
                    PublicationCount: response.Publications.Length
                )
            ),
        };
    }
}
