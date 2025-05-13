using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullResetFunction(IFullSearchableDocumentResetter fullSearchableDocumentResetter)
{
    [Function(nameof(FullSearchableDocumentsReset))]
    public async Task<FullSearchableDocumentsResetOutput> FullSearchableDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(FullSearchableDocumentsReset))]
        HttpRequest _,
        FunctionContext context)
    {
        var response = await fullSearchableDocumentResetter.PerformReset(context.CancellationToken);
        
        return new FullSearchableDocumentsResetOutput
        {
            RefreshSearchableDocuments = response.AllPublications
                .Select(publication => new RefreshSearchableDocumentMessageDto
                                                    {
                                                        PublicationSlug = publication.PublicationSlug
                                                    })
                .ToArray()
        };
    }
}

public class FullSearchableDocumentsResetOutput
{
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public RefreshSearchableDocumentMessageDto[] RefreshSearchableDocuments { get; init; }
}
