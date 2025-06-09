using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;

public class SearchDocumentCheckFunction(SearchableDocumentChecker searchableDocumentChecker)
{
    [Function(nameof(SearchDocumentCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> SearchDocumentCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        #pragma warning disable IDE0060 // Suppress removing unused parameter - must have a valid binding name for Azure function
        HttpRequest httpRequest,
        #pragma warning restore IDE0060
        CancellationToken cancellationToken)
    {
        var report = await searchableDocumentChecker.RunCheck(cancellationToken);
        return new OkObjectResult(report);
    }
}
