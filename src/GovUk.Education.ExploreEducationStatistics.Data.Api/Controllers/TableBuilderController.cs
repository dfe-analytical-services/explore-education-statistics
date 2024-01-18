using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.RequestTimeoutConfigurationKeys;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        // Change this whenever there is a breaking change
        // that requires cache invalidation.
        public const string ApiVersion = "1";

        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IDataBlockService _dataBlockService;
        private readonly IReleaseRepository _releaseRepository;
        private readonly ITableBuilderService _tableBuilderService;

        public TableBuilderController(
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IDataBlockService dataBlockService,
            IReleaseRepository releaseRepository,
            ITableBuilderService tableBuilderService)
        {
            _contentPersistenceHelper = contentPersistenceHelper;
            _dataBlockService = dataBlockService;
            _releaseRepository = releaseRepository;
            _tableBuilderService = tableBuilderService;
        }

        [HttpPost("tablebuilder")]
        [Produces("application/json", "text/csv")]
        [CancellationTokenTimeout(TableBuilderQuery)]
        public async Task Query(
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken = default)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(ContentTypes.Csv);

                await _tableBuilderService.QueryToCsvStream(query, Response.BodyWriter.AsStream(), cancellationToken);

                return;
            }

            var result = await _tableBuilderService
                .Query(query, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        [HttpPost("tablebuilder/release/{releaseId:guid}")]
        [Produces("application/json", "text/csv")]
        [CancellationTokenTimeout(TableBuilderQuery)]
        public async Task Query(
            Guid releaseId,
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken = default)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(
                    contentType: ContentTypes.Csv,
                    filename: $"{releaseId}.csv");

                await _tableBuilderService.QueryToCsvStream(
                    releaseId,
                    query,
                    Response.BodyWriter.AsStream(),
                    cancellationToken
                );

                return;
            }

            var result = await _tableBuilderService
                .Query(releaseId, query, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        // Note that releaseId is not necessary for this method to function any more in the Data API, but remains in place
        // both in order to support legacy URLs in bookmarks and in content, but also to remain consistent with the equivalent
        // endpoint in the Admin API, which does require the Release Id in order to differentiate between different 
        // DataBlockVersions rather than simply picking the latest published one.
        [HttpGet("tablebuilder/release/{releaseId:guid}/data-block/{dataBlockParentId:guid}")]
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

        [HttpGet("tablebuilder/fast-track/{dataBlockParentId:guid}")]
        public async Task<ActionResult<FastTrackViewModel>> QueryForFastTrack(Guid dataBlockParentId)
        {
            return await GetLatestPublishedDataBlockVersion(dataBlockParentId)
                .OnSuccessCombineWith(GetDataBlockTableResult)
                .OnSuccessCombineWith(tuple => _releaseRepository.GetLatestPublishedRelease(tuple.Item1.Release.PublicationId))
                .OnSuccess(tuple =>
                {
                    var (dataBlockVersion, tableResult, latestRelease) = tuple;
                    return BuildFastTrackViewModel(dataBlockVersion, tableResult, latestRelease);
                })
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(DataBlockTableResultCacheKey))]
        private Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
            DataBlockVersion dataBlockVersion)
        {
            return _dataBlockService.GetDataBlockTableResult(
                releaseId: dataBlockVersion.ReleaseId,
                dataBlockVersionId: dataBlockVersion.Id);
        }

        private static FastTrackViewModel BuildFastTrackViewModel(
            DataBlockVersion dataBlockVersion,
            TableBuilderResultViewModel tableResult,
            Release latestRelease)
        {
            var release = dataBlockVersion.Release;
            var dataBlock = dataBlockVersion.ContentBlock;

            return new FastTrackViewModel
            {
                DataBlockParentId = dataBlockVersion.DataBlockParentId,
                Configuration = dataBlock.Table,
                FullTable = tableResult,
                Query = new TableBuilderQueryViewModel(release.PublicationId, dataBlock.Query),
                ReleaseId = release.Id,
                ReleaseSlug = release.Slug,
                ReleaseType = release.Type,
                LatestData = latestRelease.Id == release.Id,
                LatestReleaseTitle = latestRelease.Title
            };
        }

        // Get the latest published DataBlockVersion for the given parent id, also including its Release and
        // Publication for the purposes of generating a cache key for Data Block table results.
        private Task<Either<ActionResult, DataBlockVersion>> GetLatestPublishedDataBlockVersion(Guid dataBlockParentId)
        {
            return _contentPersistenceHelper
                .CheckEntityExists<DataBlockParent>(dataBlockParentId, q => q
                    .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                    .ThenInclude(dataBlockVersion => dataBlockVersion.Release)
                    .ThenInclude(release => release.Publication))
                .OnSuccess(dataBlock => dataBlock.LatestPublishedVersion)
                .OrNotFound();
        }
    }
}
