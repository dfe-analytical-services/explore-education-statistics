using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public class FullResetFunction(IFullSearchableDocumentReseter fullSearchableDocumentReseter)
{
    [Function(nameof(FullSearchableDocumentsReset))]
    public async Task<FullSearchableDocumentsResetOutput> FullSearchableDocumentsReset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(FullSearchableDocumentsReset))]
        HttpRequest _,
        FunctionContext context)
    {
        var response = await fullSearchableDocumentReseter.Run(context.CancellationToken);
        
        return new FullSearchableDocumentsResetOutput
        {
            RefreshSearchableDocuments = response.AllPublicationSlugs
                .Select(publicationSlug => new RefreshSearchableDocumentMessageDto
                                                    {
                                                        PublicationSlug = publicationSlug
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
