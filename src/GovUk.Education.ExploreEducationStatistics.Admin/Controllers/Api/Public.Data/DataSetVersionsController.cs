#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
    public async Task<ActionResult<DataSetVersionSummaryViewModel>> CreateNextVersion(
        [FromBody] NextDataSetVersionCreateRequest nextDataSetVersionCreateRequest,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .CreateNextVersion(
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

    [HttpGet("{dataSetVersionId:guid}/changes")]
    [Produces("application/json")]
    public async Task<ActionResult> GetVersionChanges(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        // We use a streaming approach as we don't have to share the public API view models
        // with the admin. This means we can avoid inefficiently re-serializing a second JSON
        // response and don't need to load the original response into memory.
        return await dataSetVersionService
            .GetVersionChanges(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken)
            .OnSuccessVoid(async response =>
            {
                Response.ContentType = response.Content.Headers.ContentType?.ToString();
                await response.Content.CopyToAsync(Response.BodyWriter.AsStream(), cancellationToken);
            })
            .HandleFailuresOrNoOp();
    }
}
