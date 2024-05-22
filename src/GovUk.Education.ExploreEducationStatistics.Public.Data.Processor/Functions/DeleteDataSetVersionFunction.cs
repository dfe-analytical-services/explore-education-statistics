using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class DeleteDataSetVersionFunction(IDataSetVersionService dataSetVersionService)
{
    [Function(nameof(DeleteDataSetVersion))]
    public async Task<IActionResult> DeleteDataSetVersion(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = $"{nameof(DeleteDataSetVersion)}/{{dataSetVersionId}}")] HttpRequest httpRequest,
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService.DeleteVersion(
                dataSetVersionId,
                cancellationToken: cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }
}
