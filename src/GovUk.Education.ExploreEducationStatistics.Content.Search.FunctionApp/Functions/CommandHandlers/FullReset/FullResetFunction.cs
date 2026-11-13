using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullResetFunction(IFullReleaseDocumentResetter fullReleaseDocumentResetter, ICommandHandler commandHandler)
{
    [Function(nameof(FullReleaseDocumentsReset))]
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public async Task<RefreshReleaseDocumentMessageDto[]> FullReleaseDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(FullReleaseDocumentsReset))]
            HttpRequest ignored, //  The binding name _ is invalid
        FunctionContext context
    ) => await commandHandler.Handle(ResetReleaseDocument, context.CancellationToken);

    private async Task<RefreshReleaseDocumentMessageDto[]> ResetReleaseDocument(CancellationToken cancellationToken)
    {
        var response = await fullReleaseDocumentResetter.PerformReset(cancellationToken);

        return response
            .AllPublications.Select(publication => new RefreshReleaseDocumentMessageDto
            {
                PublicationSlug = publication.PublicationSlug,
            })
            .ToArray();
    }
}
