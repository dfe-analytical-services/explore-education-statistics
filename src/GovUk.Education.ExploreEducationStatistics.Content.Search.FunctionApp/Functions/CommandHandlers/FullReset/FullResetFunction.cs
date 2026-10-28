using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullResetFunction(IFullReleaseDocumentResetter fullReleaseDocumentResetter, ICommandHandler commandHandler)
{
    [Function(nameof(FullSearchableDocumentsReset))]
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public async Task<RefreshReleaseDocumentMessageDto[]> FullSearchableDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(FullSearchableDocumentsReset))]
            HttpRequest ignored, //  The binding name _ is invalid
        FunctionContext context
    ) => await commandHandler.Handle(ResetSearchableDocument, context.CancellationToken);

    private async Task<RefreshReleaseDocumentMessageDto[]> ResetSearchableDocument(CancellationToken cancellationToken)
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
