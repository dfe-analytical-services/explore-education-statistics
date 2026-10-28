using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RemoveReleaseDocument;

public class RemoveReleaseDocumentFunction(
    ILogger<RemoveReleaseDocumentFunction> logger,
    ISearchableDocumentRemover searchableDocumentRemover,
    ICommandHandler commandHandler
)
{
    [Function(nameof(RemoveReleaseDocument))]
    public async Task RemoveReleaseDocument(
        [QueueTrigger("%RemoveReleaseDocumentQueueName%")] RemoveReleaseDocumentDto message,
        FunctionContext context
    ) => await commandHandler.Handle(RemoveReleaseDocument, message, context.CancellationToken);

    private async Task RemoveReleaseDocument(RemoveReleaseDocumentDto message, CancellationToken cancellationToken)
    {
        var releaseId = message.ReleaseId;
        if (releaseId.IsBlank())
        {
            return;
        }

        var response = await searchableDocumentRemover.RemoveSearchableDocument(
            new RemoveSearchableDocumentRequest { ReleaseId = releaseId.Value },
            cancellationToken
        );

        logger.LogInformation(
            """Removed searchable document "{ReleaseId}". Response: {@response}""",
            releaseId,
            response
        );
    }
}
