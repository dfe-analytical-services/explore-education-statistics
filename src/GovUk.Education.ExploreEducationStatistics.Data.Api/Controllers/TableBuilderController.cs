using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.RequestTimeoutConfigurationKeys;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;

[Route("api")]
[ApiController]
public class TableBuilderController(
    ContentDbContext contextDbContext,
    IDataBlockService dataBlockService,
    ITableBuilderService tableBuilderService)
    : ControllerBase
{
    // Change this whenever there is a breaking change
    // that requires cache invalidation.
    public const string ApiVersion = "1";

    [HttpPost("tablebuilder")]
    [Produces("application/json", "text/csv")]
    [CancellationTokenTimeout(TableBuilderQuery)]
    public async Task<ActionResult> Query(
        [FromBody] FullTableQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (Request.AcceptsCsv(exact: true))
        {
            Response.ContentDispositionAttachment(ContentTypes.Csv);

            return await tableBuilderService.QueryToCsvStream(
                    query: request.AsFullTableQuery(),
                    stream: Response.BodyWriter.AsStream(),
                    cancellationToken: cancellationToken
                )
                .HandleFailuresOrNoOp();
        }

        return await tableBuilderService
            .Query(request.AsFullTableQuery(), cancellationToken)
            .HandleFailuresOr(Ok);
    }

    [HttpPost("tablebuilder/release/{releaseVersionId:guid}")]
    [Produces("application/json", "text/csv")]
    [CancellationTokenTimeout(TableBuilderQuery)]
    public async Task<ActionResult> Query(
        Guid releaseVersionId,
        [FromBody] FullTableQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (Request.AcceptsCsv(exact: true))
        {
            Response.ContentDispositionAttachment(
                contentType: ContentTypes.Csv,
                filename: $"{releaseVersionId}.csv");

            return await tableBuilderService.QueryToCsvStream(
                    releaseVersionId: releaseVersionId,
                    query: request.AsFullTableQuery(),
                    stream: Response.BodyWriter.AsStream(),
                    cancellationToken: cancellationToken
                )
                .HandleFailuresOrNoOp();
        }

        return await tableBuilderService
            .Query(releaseVersionId, request.AsFullTableQuery(), cancellationToken)
            .HandleFailuresOr(Ok);
    }

    // Note that releaseVersionId is not necessary for this method to function any more in the Data API, but remains in place
    // both in order to support legacy URLs in bookmarks and in content, but also to remain consistent with the equivalent
    // endpoint in the Admin API, which does require the Release Id in order to differentiate between different 
    // DataBlockVersions rather than simply picking the latest published one.
    [HttpGet("tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}")]
    public async Task<ActionResult<TableBuilderResultViewModel>> QueryForTableBuilderResult(
        Guid dataBlockParentId)
    {
        var actionResult = await GetLatestPublishedDataBlockVersion(dataBlockParentId)
            .OnSuccessDo(dataBlockVersion => this
                .CacheWithLastModifiedAndETag(lastModified: dataBlockVersion.Published, ApiVersion))
            .OnSuccess(GetDataBlockTableResult)
            .HandleFailuresOrOk();

        if (actionResult.Result is not NotFoundResult)
        {
            Response.Headers["Cache-Control"] = "public,max-age=300";
        }

        return actionResult;
    }

    [HttpGet("tablebuilder/release/{releaseVersionId:guid}/data-block/{dataBlockParentId:guid}/geojson")]
    public async Task<ActionResult<Dictionary<string, List<LocationAttributeViewModel>>>> QueryForDataBlockWithGeoJsonResult(
        Guid dataBlockParentId,
        [FromQuery] long boundaryLevelId)
    {
        var actionResult = await GetLatestPublishedDataBlockVersion(dataBlockParentId)
            .OnSuccessDo(dataBlockVersion => this
                .CacheWithLastModifiedAndETag(lastModified: dataBlockVersion.Published, ApiVersion))
            .OnSuccess(dataBlockVersion => GetLocations(dataBlockVersion, boundaryLevelId))
            .HandleFailuresOrOk();

        if (actionResult.Result is not NotFoundResult)
        {
            Response.Headers.CacheControl = "public,max-age=300";
        }

        return actionResult;
    }

    [HttpGet("tablebuilder/fast-track/{dataBlockParentId:guid}")]
    public async Task<ActionResult<FastTrackViewModel>> QueryForFastTrack(Guid dataBlockParentId)
    {
        return await GetLatestPublishedDataBlockVersion(dataBlockParentId)
            .OnSuccessCombineWith(dataBlockVersion => GetDataBlockTableResult(dataBlockVersion))
            .OnSuccessCombineWith(async tuple =>
            {
                var (dataBlockVersion, _) = tuple;
                return await GetLatestPublishedReleaseVersion(dataBlockVersion);
            })
            .OnSuccess(tuple =>
            {
                var (dataBlockVersion, tableResult, latestReleaseVersion) = tuple;
                return BuildFastTrackViewModel(dataBlockVersion, tableResult, latestReleaseVersion);
            })
            .HandleFailuresOrOk();
    }

    [BlobCache(typeof(DataBlockTableResultCacheKey))]
    private Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
        DataBlockVersion dataBlockVersion)
    {
        return dataBlockService.GetDataBlockTableResult(
            releaseVersionId: dataBlockVersion.ReleaseVersionId,
            dataBlockVersionId: dataBlockVersion.Id);
    }

    [BlobCache(typeof(LocationsForDataBlockCacheKey))]
    private Task<Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>> GetLocations(
        DataBlockVersion dataBlockVersion,
        long boundaryLevelId)
    {
        return dataBlockService.GetLocationsForDataBlock(
            releaseVersionId: dataBlockVersion.ReleaseVersionId,
            dataBlockVersionId: dataBlockVersion.Id,
            boundaryLevelId);
    }

    private static FastTrackViewModel BuildFastTrackViewModel(
        DataBlockVersion dataBlockVersion,
        TableBuilderResultViewModel tableResult,
        ReleaseVersion latestReleaseVersion)
    {
        var releaseVersion = dataBlockVersion.ReleaseVersion;
        var dataBlock = dataBlockVersion.ContentBlock;

        return new FastTrackViewModel
        {
            DataBlockParentId = dataBlockVersion.DataBlockParentId,
            Configuration = dataBlock.Table,
            FullTable = tableResult,
            Query = new TableBuilderQueryViewModel(releaseVersion.Release.PublicationId, dataBlock.Query),
            ReleaseId = releaseVersion.Id,
            ReleaseSlug = releaseVersion.Release.Slug,
            ReleaseType = releaseVersion.Type,
            LatestData = latestReleaseVersion.Id == releaseVersion.Id,
            LatestReleaseTitle = latestReleaseVersion.Release.Title,
            LatestReleaseSlug = latestReleaseVersion.Release.Slug
        };
    }

    // Get the latest published DataBlockVersion for the given parent id, also including its ReleaseVersion and
    // Publication for the purposes of generating a cache key for Data Block table results.
    private async Task<Either<ActionResult, DataBlockVersion>> GetLatestPublishedDataBlockVersion(
        Guid dataBlockParentId)
    {
        return await contextDbContext.DataBlockParents
            .Include(dbp => dbp.LatestPublishedVersion)
            .ThenInclude(dbv => dbv.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(dbp => dbp.Id == dataBlockParentId)
            .OnSuccess(dbp => dbp.LatestPublishedVersion)
            .OrNotFound();
    }

    private async Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersion(
        DataBlockVersion dataBlockVersion)
    {
        return await contextDbContext.Publications
            .Include(p => p.LatestPublishedReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(p => p.Id == dataBlockVersion.ReleaseVersion.Release.PublicationId)
            .OnSuccess(p => p.LatestPublishedReleaseVersion)
            .OrNotFound();
    }
}
