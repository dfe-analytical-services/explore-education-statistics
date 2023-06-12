using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.RequestTimeoutConfigurationKeys;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserService _userService;

        public TableBuilderController(
            ITableBuilderService tableBuilderService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService)
        {
            _tableBuilderService = tableBuilderService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
        }

        [HttpPost("data/tablebuilder/release/{releaseId:guid}")]
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

        [HttpGet("data/tablebuilder/release/{releaseId:guid}/data-block/{dataBlockId:guid}")]
        public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
            Guid releaseId,
            Guid dataBlockId,
            CancellationToken cancellationToken = default)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<ReleaseContentBlock>(
                    query => query
                        .Include(rcb => rcb.ContentBlock)
                        .Include(rcb => rcb.Release)
                        .Where(
                            rcb => rcb.ReleaseId == releaseId
                                   && rcb.ContentBlockId == dataBlockId
                        )
                )
                .OnSuccess(block => GetReleaseDataBlockResults(block, cancellationToken))
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(DataBlockTableResultCacheKey))]
        private async Task<Either<ActionResult, TableBuilderResultViewModel>> GetReleaseDataBlockResults(
            ReleaseContentBlock block, 
            CancellationToken cancellationToken)
        {
            if (!(block.ContentBlock is DataBlock dataBlock))
            {
                return new NotFoundResult();
            }
            
            return await _userService
                .CheckCanViewRelease(block.Release)
                .OnSuccess(_ => _tableBuilderService.Query(block.ReleaseId, dataBlock.Query, cancellationToken));
        }
    }
}
