using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class UnfinaliseDataSetVersionFunction(
    IDataSetVersionService dataSetVersionService,
    ILogger<UnfinaliseDataSetVersionFunction> logger
)
{
    [Function(nameof(UnfinaliseDataSetVersion))]
    public async Task<IActionResult> UnfinaliseDataSetVersion(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = $"{nameof(UnfinaliseDataSetVersion)}/{{dataSetVersionId:guid}}"
        )]
            HttpRequest request,
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await dataSetVersionService
                .UnfinaliseVersion(dataSetVersionId, cancellationToken)
                .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception occurred while executing '{FunctionName}'",
                nameof(UnfinaliseDataSetVersion)
            );
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
