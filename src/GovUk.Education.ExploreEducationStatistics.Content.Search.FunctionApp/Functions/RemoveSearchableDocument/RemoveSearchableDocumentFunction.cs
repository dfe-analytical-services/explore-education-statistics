using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.RemoveSearchableDocument;

public class RemoveSearchableDocumentFunction(
    ILogger<RemoveSearchableDocumentFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover)
{
    [Function(nameof(RemoveSearchableDocument))]
    public async Task RemoveSearchableDocument(
        [QueueTrigger("%RemoveSearchableDocumentQueueName%")] RemoveSearchableDocumentDto message,
        FunctionContext context)
    {
        var releaseId = message.ReleaseId;
        if (releaseId is null || releaseId == Guid.Empty)
        {
            return;
        }

        var response = await searchableDocumentRemover.RemoveSearchableDocument(
            new RemoveSearchableDocumentRequest { ReleaseId = releaseId.Value },
            context.CancellationToken);

        logger.LogInformation(
            "Removed searchable document \"{ReleaseId}\". Response: {@response}", releaseId, response);
    }
}
