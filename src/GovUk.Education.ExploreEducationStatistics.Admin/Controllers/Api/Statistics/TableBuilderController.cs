using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.RequestTimeoutConfigurationKeys;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
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
            IDataBlockService dataBlockService)
        {
            _tableBuilderService = tableBuilderService;
            _userService = userService;
            _dataBlockService = dataBlockService;
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

        [HttpGet("data/tablebuilder/release/{releaseId:guid}/data-block/{dataBlockParentId:guid}")]
        public async Task<ActionResult<TableBuilderResultViewModel>> QueryForDataBlock(
            Guid releaseId,
            Guid dataBlockParentId,
            CancellationToken cancellationToken = default)
        {
            return await _dataBlockService
                .GetDataBlockVersionForRelease(releaseId: releaseId, dataBlockParentId: dataBlockParentId)
                .OnSuccess(dataBlockVersion => GetReleaseDataBlockResults(dataBlockVersion, cancellationToken))
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(DataBlockTableResultCacheKey))]
        private async Task<Either<ActionResult, TableBuilderResultViewModel>> GetReleaseDataBlockResults(
            DataBlockVersion dataBlockVersion,
            CancellationToken cancellationToken)
        {
            return await _userService
                .CheckCanViewRelease(dataBlockVersion.Release)
                .OnSuccess(_ => _tableBuilderService.Query(dataBlockVersion.ReleaseId, dataBlockVersion.Query, cancellationToken));
        }
    }
}
