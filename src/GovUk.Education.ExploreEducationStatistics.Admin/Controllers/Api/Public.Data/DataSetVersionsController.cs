#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-versions")]
public class DataSetVersionsController(IDataSetVersionService dataSetVersionService) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetVersionSummaryViewModel>> CreateNextDataSetVersion(
        [FromBody] NextDataSetVersionCreateRequest nextDataSetVersionCreateRequest,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .CreateNextDataSetVersion(
                releaseFileId: nextDataSetVersionCreateRequest.ReleaseFileId,
                dataSetId: nextDataSetVersionCreateRequest.DataSetId,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpDelete("{dataSetVersionId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .DeleteVersion(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }
}
