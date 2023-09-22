using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
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

        [ResponseCache(Duration = 300)]
        [HttpGet("tablebuilder/release/{releaseId:guid}/data-block/{dataBlockId:guid}")]
        public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
            Guid releaseId,
            Guid dataBlockId)
        {
            // TODO DW - we're not actually caching the resulting "CacheableDataBlock" here (although we are its
            // GetDataBlockTableResult)
            return await GetCacheableDataBlock(
                    releaseId: releaseId,
                    dataBlockId: dataBlockId)
                .OnSuccessDo(cacheable => this.CacheWithLastModifiedAndETag(cacheable.LastModified, ApiVersion))
                .OnSuccess(GetDataBlockTableResult)
                .HandleFailuresOrOk();
        }

        [HttpGet("tablebuilder/fast-track/{fastTrackId:guid}")]
        public async Task<ActionResult<FastTrackViewModel>> QueryForFastTrack(Guid fastTrackId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<DataBlockVersion>(fastTracks => fastTracks
                    .Include(fastTrackVersion => fastTrackVersion.Release)
                    .ThenInclude(release => release.Publication)
                    .Include(fastTrackVersion => fastTrackVersion.DataBlock)
                    .Where(fastTrackVersion => fastTrackVersion.FastTrackId  == fastTrackId))
                .OnSuccessCombineWith(fastTrackVersion => GetDataBlockTableResult(new CacheableFastTrack(fastTrackVersion)))
                .OnSuccessCombineWith(tuple => _releaseRepository.GetLatestPublishedRelease(tuple.Item1.Release.PublicationId))
                .OnSuccess(tuple =>
                {
                    var (fastTrackVersion, tableResult, latestRelease) = tuple;
                    return BuildFastTrackViewModel(fastTrackVersion, tableResult, latestRelease);
                })
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(DataBlockTableResultCacheKey))]
        private Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
            CacheableFastTrack cacheable)
        {
            // TODO EES-3363 The CacheableDataBlock parameter type exists to provide the Release and Publication slugs
            // required in the cache key.
            // In future we should change the storage path for public cached items to use a directory structure
            // of Release id's so that we don't need to lookup the Release and Publication to use the slugs.
            return _dataBlockService.GetDataBlockTableResult(
                releaseId: cacheable.ReleaseId,
                dataBlockId: cacheable.FastTrackId);
        }

        private static FastTrackViewModel BuildFastTrackViewModel(
            DataBlockVersion dataBlockVersion,
            TableBuilderResultViewModel tableResult,
            Release latestRelease)
        {
            var release = dataBlockVersion.Release;
            var dataBlock = dataBlockVersion.DataBlock;
            
            return new FastTrackViewModel
            {
                Id = dataBlockVersion.DataBlockId,
                Configuration = dataBlock.Table,
                FullTable = tableResult,
                Query = new TableBuilderQueryViewModel(release.PublicationId, dataBlock.Query),
                ReleaseId = release.Id,
                ReleaseSlug = release.Slug,
                LatestData = latestRelease.Id == release.Id,
                LatestReleaseTitle = latestRelease.Title
            };
        }

        private async Task<Either<ActionResult, CacheableFastTrack>> GetCacheableDataBlock(
            Guid releaseId,
            Guid dataBlockId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId,
                    q => q.Include(release => release.Publication))
                .OnSuccess(release => new CacheableFastTrack(dataBlockId, release));
        }
    }
}
