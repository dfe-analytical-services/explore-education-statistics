using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class DeleteDataSetVersionFunction(
    IDataSetVersionService dataSetVersionService,
    ILogger<DeleteDataSetVersionFunction> logger
)
{
    [Function(nameof(DeleteDataSetVersion))]
    public async Task<IActionResult> DeleteDataSetVersion(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "delete",
            Route = $"{nameof(DeleteDataSetVersion)}/{{dataSetVersionId:guid}}"
        )]
            HttpRequest request,
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await dataSetVersionService
                .DeleteVersion(dataSetVersionId, cancellationToken: cancellationToken)
                .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
        }
        catch (Exception ex)
        {
            logger.LogError(
                exception: ex,
                "Exception occured while executing '{FunctionName}'",
                nameof(DeleteDataSetVersion)
            );
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
