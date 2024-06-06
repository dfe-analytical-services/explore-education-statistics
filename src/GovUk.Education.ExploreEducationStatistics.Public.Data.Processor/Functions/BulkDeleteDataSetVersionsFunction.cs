using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class BulkDeleteDataSetVersionsFunction(
    IDataSetVersionService dataSetVersionService,
    ILogger<BulkDeleteDataSetVersionsFunction> logger)
{
    [Function(nameof(BulkDeleteDataSetVersions))]
    public async Task<IActionResult> BulkDeleteDataSetVersions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
            Route = $"{nameof(BulkDeleteDataSetVersions)}/{{releaseVersionId}}")]
        HttpRequest httpRequest,
        Guid releaseVersionId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dataSetVersionService.BulkDeleteVersions(
                    releaseVersionId,
                    cancellationToken: cancellationToken)
                .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Exception occured while executing '{FunctionName}'", nameof(BulkDeleteDataSetVersionsFunction));
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
