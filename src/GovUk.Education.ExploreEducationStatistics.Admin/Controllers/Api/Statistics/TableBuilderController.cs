using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class TableBuilderController(
    ITableBuilderService tableBuilderService,
    IUserService userService,
    IDataBlockService dataBlockService,
    IPrivateBlobCacheService privateBlobCacheService,
    ILogger<TableBuilderController> logger
) : ControllerBase
{
    [HttpPost("data/tablebuilder/release/{releaseVersionId:guid}")]
    [Produces("application/json", "text/csv")]
    public async Task<ActionResult> Query(
        Guid releaseVersionId,
        [FromBody] FullTableQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        using var cancellationTokenWithTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationTokenWithTimeout.CancelAfter(TimeSpan.FromMinutes(3.5));

        if (Request.AcceptsCsv(exact: true))
        {
            Response.ContentDispositionAttachment(contentType: ContentTypes.Csv, filename: $"{releaseVersionId}.csv");

            return await tableBuilderService
                .QueryToCsvStream(
                    releaseVersionId: releaseVersionId,
                    query: request.AsFullTableQuery(),
                    stream: Response.BodyWriter.AsStream(),
                    cancellationToken: cancellationTokenWithTimeout.Token
                )
                .HandleFailuresOrNoOp();
        }

        return await tableBuilderService
            .Query(releaseVersionId, request.AsFullTableQuery(enableCropping: true), cancellationTokenWithTimeout.Token)
            .HandleFailuresOr(Ok);
    }

    [HttpGet("data/tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}")]
    public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
        Guid releaseVersionId,
        Guid dataBlockParentId,
        CancellationToken cancellationToken = default
    )
    {
        return await dataBlockService
            .GetDataBlockVersionForRelease(releaseVersionId: releaseVersionId, dataBlockParentId: dataBlockParentId)
            .OnSuccess(dataBlockVersion => GetReleaseDataBlockResults(dataBlockVersion, cancellationToken))
            .HandleFailuresOrOk();
    }

    [HttpGet("data/tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}/geojson")]
    public async Task<ActionResult<Dictionary<string, List<LocationAttributeViewModel>>>> QueryForDataBlockWithGeoJson(
        Guid releaseVersionId,
        Guid dataBlockParentId,
        [FromQuery] long boundaryLevelId,
        CancellationToken cancellationToken = default
    )
    {
        return await dataBlockService
            .GetDataBlockVersionForRelease(releaseVersionId, dataBlockParentId)
            .OnSuccess(dataBlockVersion =>
                GetLocations(releaseVersionId, dataBlockVersion, boundaryLevelId, cancellationToken)
            )
            .HandleFailuresOrOk();
    }

    private Task<Either<ActionResult, TableBuilderResultViewModel>> GetReleaseDataBlockResults(
        DataBlockVersion dataBlockVersion,
        CancellationToken cancellationToken
    )
    {
        return privateBlobCacheService.GetOrCreateAsync(
            cacheKey: new DataBlockTableResultCacheKey(dataBlockVersion),
            createIfNotExistsFn: () =>
                userService
                    .CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion)
                    .OnSuccess(_ =>
                        tableBuilderService.Query(
                            releaseVersionId: dataBlockVersion.ReleaseVersionId,
                            dataBlockVersion.Query,
                            cancellationToken
                        )
                    ),
            logger: logger
        );
    }

    private Task<Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>> GetLocations(
        Guid releaseVersionId,
        DataBlockVersion dataBlockVersion,
        long boundaryLevelId,
        CancellationToken cancellationToken
    )
    {
        return privateBlobCacheService.GetOrCreateAsync(
            cacheKey: new LocationsForDataBlockCacheKey(
                dataBlockVersion: dataBlockVersion,
                boundaryLevelId: boundaryLevelId
            ),
            createIfNotExistsFn: () =>
                userService
                    .CheckCanViewReleaseVersion(dataBlockVersion.ReleaseVersion)
                    .OnSuccess(_ =>
                        tableBuilderService.QueryForBoundaryLevel(
                            releaseVersionId,
                            dataBlockVersion.Query,
                            boundaryLevelId,
                            cancellationToken
                        )
                    ),
            logger: logger
        );
    }
}
