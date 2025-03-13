#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .ListLiveVersions(
                dataSetId: request.DataSetId,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

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

    [HttpPost]
    [Produces("application/json")]
    [Route("complete")]
    public async Task<ActionResult<DataSetVersionSummaryViewModel>> CompleteNextVersionImport(
        [FromBody] NextDataSetVersionCompleteImportRequest nextDataSetVersionCompleteImportRequest,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .CompleteNextVersionImport(
                dataSetVersionId: nextDataSetVersionCompleteImportRequest.DataSetVersionId,
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
    public async Task<ActionResult<DataSetVersionChangesViewModel>> GetVersionChanges(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .GetVersionChanges(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPatch("{dataSetVersionId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetDraftVersionViewModel>> UpdateVersion(
        Guid dataSetVersionId,
        [FromBody] DataSetVersionUpdateRequest dataSetVersionUpdateRequest,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionService
            .UpdateVersion(
                dataSetVersionId: dataSetVersionId,
                updateRequest: dataSetVersionUpdateRequest,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
