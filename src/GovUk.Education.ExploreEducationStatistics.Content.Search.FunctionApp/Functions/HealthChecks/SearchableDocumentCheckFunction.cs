using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
#pragma warning disable IDE0060 // Suppress removing unused parameter `ignored` - must have a valid binding name for Azure function

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;

public class SearchableDocumentCheckFunction(SearchableDocumentChecker searchableDocumentChecker)
{
    [Function(nameof(SearchableDocumentCheck))]
    [Produces("application/json")]
    public async Task<IActionResult> SearchableDocumentCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest ignored, //  The binding name _ is invalid
        CancellationToken cancellationToken
    )
    {
        var report = await searchableDocumentChecker.RunCheck(cancellationToken);
        return new OkObjectResult(report);
    }
}
