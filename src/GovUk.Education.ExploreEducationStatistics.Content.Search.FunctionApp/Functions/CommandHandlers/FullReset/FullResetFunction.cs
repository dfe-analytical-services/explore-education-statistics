using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress Remove unused parameter HttpRequest

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullResetFunction(
    IFullSearchableDocumentResetter fullSearchableDocumentResetter,
    ICommandHandler commandHandler)
{
    [Function(nameof(FullSearchableDocumentsReset))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> FullSearchableDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(FullSearchableDocumentsReset))]
        HttpRequest ignored, // The binding name _ is invalid
        FunctionContext context) =>
        await commandHandler.Handle(
            ResetSearchableDocument,
            context.CancellationToken) ?? [];

    private async Task<RefreshSearchableDocumentMessageDto[]> ResetSearchableDocument(CancellationToken cancellationToken)
    {
        var response = await fullSearchableDocumentResetter.PerformReset(cancellationToken);

        return response.AllPublications
            .Select(publication =>
                    new RefreshSearchableDocumentMessageDto { PublicationSlug = publication.PublicationSlug })
            .ToArray();
    }
}
