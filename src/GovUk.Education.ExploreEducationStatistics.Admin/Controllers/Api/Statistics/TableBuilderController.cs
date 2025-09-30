using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.RequestTimeoutConfigurationKeys;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class TableBuilderController : ControllerBase
{
    private readonly ITableBuilderService _tableBuilderService;
    private readonly IUserService _userService;
    private readonly IDataBlockService _dataBlockService;

    public TableBuilderController(
        ITableBuilderService tableBuilderService,
        IUserService userService,
        IDataBlockService dataBlockService
    )
    {
        _tableBuilderService = tableBuilderService;
        _userService = userService;
        _dataBlockService = dataBlockService;
    }

    [HttpPost("data/tablebuilder/release/{releaseVersionId:guid}")]
    [Produces("application/json", "text/csv")]
    [CancellationTokenTimeout(TableBuilderQuery)]
    public async Task<ActionResult> Query(
        Guid releaseVersionId,
        [FromBody] FullTableQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (Request.AcceptsCsv(exact: true))
        {
            Response.ContentDispositionAttachment(
                contentType: ContentTypes.Csv,
                filename: $"{releaseVersionId}.csv"
            );

            return await _tableBuilderService
                .QueryToCsvStream(
                    releaseVersionId: releaseVersionId,
                    query: request.AsFullTableQuery(),
                    stream: Response.BodyWriter.AsStream(),
                    cancellationToken: cancellationToken
                )
                .HandleFailuresOrNoOp();
        }

        return await _tableBuilderService
            .Query(releaseVersionId, request.AsFullTableQuery(), cancellationToken)
            .HandleFailuresOr(Ok);
    }

    [HttpGet(
        "data/tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}"
    )]
    public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
        Guid releaseVersionId,
        Guid dataBlockParentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataBlockService
            .GetDataBlockVersionForRelease(
                releaseVersionId: releaseVersionId,
                dataBlockParentId: dataBlockParentId
            )
            .OnSuccess(dataBlockVersion =>
                GetReleaseDataBlockResults(dataBlockVersion, cancellationToken)
            )
            .HandleFailuresOrOk();
    }

    [HttpGet(
        "data/tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}/geojson"
    )]
    public async Task<
        ActionResult<Dictionary<string, List<LocationAttributeViewModel>>>
    > QueryForDataBlockWithGeoJson(
        Guid releaseVersionId,
        Guid dataBlockParentId,
        [FromQuery] long boundaryLevelId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dataBlockService
            .GetDataBlockVersionForRelease(releaseVersionId, dataBlockParentId)
            .OnSuccess(dataBlockVersion =>
                GetLocations(releaseVersionId, dataBlockVersion, boundaryLevelId, cancellationToken)
            )
            .HandleFailuresOrOk();
    }

    [BlobCache(typeof(DataBlockTableResultCacheKey))]
    private async Task<
        Either<ActionResult, TableBuilderResultViewModel>
    > GetReleaseDataBlockResults(
        DataBlockVersion dataBlockVersion,
        CancellationToken cancellationToken
    )
    {
        return await _userService
            .CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion)
            .OnSuccess(_ =>
                _tableBuilderService.Query(
                    releaseVersionId: dataBlockVersion.ReleaseVersionId,
                    dataBlockVersion.Query,
                    cancellationToken
                )
            );
    }

    [BlobCache(typeof(LocationsForDataBlockCacheKey))]
    private async Task<
        Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>
    > GetLocations(
        Guid releaseVersionId,
        DataBlockVersion dataBlockVersion,
        long boundaryLevelId,
        CancellationToken cancellationToken
    )
    {
        return await _userService
            .CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion)
            .OnSuccess(_ =>
                _tableBuilderService.QueryForBoundaryLevel(
                    releaseVersionId,
                    dataBlockVersion.Query,
                    boundaryLevelId,
                    cancellationToken
                )
            );
    }
}
