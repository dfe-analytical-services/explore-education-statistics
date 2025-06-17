using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveSearchableDocument;

public class RemoveSearchableDocumentFunction(
    ILogger<RemoveSearchableDocumentFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover,
    ICommandHandler commandHandler)
{
    [Function(nameof(RemoveSearchableDocument))]
    public async Task RemoveSearchableDocument(
        [QueueTrigger("%RemoveSearchableDocumentQueueName%")] RemoveSearchableDocumentDto message,
        FunctionContext context) =>
        await commandHandler.Handle(RemoveSearchableDocument, message, context.CancellationToken);
    
    private async Task RemoveSearchableDocument(RemoveSearchableDocumentDto message, CancellationToken cancellationToken)
    {
        var releaseId = message.ReleaseId;
        if (releaseId.IsBlank())
        {
            return;
        }

        var response = await searchableDocumentRemover.RemoveSearchableDocument(
            new RemoveSearchableDocumentRequest { ReleaseId = releaseId.Value },
            cancellationToken);

        logger.LogInformation(
            """Removed searchable document "{ReleaseId}". Response: {@response}""", releaseId, response);
    }
}
