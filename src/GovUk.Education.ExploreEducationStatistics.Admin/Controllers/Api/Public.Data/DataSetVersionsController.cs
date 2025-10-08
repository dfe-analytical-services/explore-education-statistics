#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-versions")]
public class DataSetVersionsController(IDataSetVersionService dataSetVersionService) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>> ListVersions(
        [FromQuery] DataSetVersionListRequest request,
        CancellationToken cancellationToken
    )
    {
        return await dataSetVersionService
            .ListLiveVersions(
                dataSetId: request.DataSetId,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    [HttpGet("{dataSetVersionId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetVersionInfoViewModel>> GetDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetVersionService
            .GetDataSetVersion(dataSetVersionId: dataSetVersionId, cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetVersionSummaryViewModel>> CreateNextVersion(
        [FromBody] NextDataSetVersionCreateRequest nextDataSetVersionCreateRequest,
        CancellationToken cancellationToken
    )
    {
        return await dataSetVersionService
            .CreateNextVersion(
                releaseFileId: nextDataSetVersionCreateRequest.ReleaseFileId,
                dataSetId: nextDataSetVersionCreateRequest.DataSetId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    [HttpPost]
    [Produces("application/json")]
    [Route("complete")]
    public async Task<ActionResult<DataSetVersionSummaryViewModel>> CompleteNextVersionImport(
        [FromBody] NextDataSetVersionCompleteImportRequest nextDataSetVersionCompleteImportRequest,
        CancellationToken cancellationToken
    )
    {
        return await dataSetVersionService
            .CompleteNextVersionImport(
                dataSetVersionId: nextDataSetVersionCompleteImportRequest.DataSetVersionId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    [HttpDelete("{dataSetVersionId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult> DeleteVersion(Guid dataSetVersionId, CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .DeleteVersion(dataSetVersionId: dataSetVersionId, cancellationToken: cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }

    [HttpGet("{dataSetVersionId:guid}/changes")]
    [Produces("application/json")]
    public async Task<ActionResult> GetVersionChanges(Guid dataSetVersionId, CancellationToken cancellationToken)
    {
        // We use a streaming approach as we don't have to share the public API view models
        // with the admin. This means we can avoid inefficiently re-serializing a second JSON
        // response and don't need to load the original response into memory.
        return await dataSetVersionService
            .GetVersionChanges(dataSetVersionId: dataSetVersionId, cancellationToken: cancellationToken)
            .OnSuccessVoid(async response =>
            {
                Response.ContentType = response.Content.Headers.ContentType?.ToString();
                await response.Content.CopyToAsync(Response.BodyWriter.AsStream(), cancellationToken);
            })
            .HandleFailuresOrNoOp();
    }

    [HttpPatch("{dataSetVersionId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetDraftVersionViewModel>> UpdateVersion(
        Guid dataSetVersionId,
        [FromBody] DataSetVersionUpdateRequest dataSetVersionUpdateRequest,
        CancellationToken cancellationToken
    )
    {
        return await dataSetVersionService
            .UpdateVersion(
                dataSetVersionId: dataSetVersionId,
                updateRequest: dataSetVersionUpdateRequest,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }
}
