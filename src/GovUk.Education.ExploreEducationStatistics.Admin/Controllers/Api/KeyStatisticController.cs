#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class KeyStatisticController : ControllerBase
    {
        private readonly IKeyStatisticService _keyStatisticService;

        public KeyStatisticController(IKeyStatisticService keyStatisticService)
        {
            _keyStatisticService = keyStatisticService;
        }

        [HttpPost("release/{releaseId}/key-statistic-data-block")]
        public async Task<ActionResult<KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
            Guid releaseId,
            KeyStatisticDataBlockCreateRequest request)
        {
            return await _keyStatisticService
                .CreateKeyStatisticDataBlock(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/key-statistic-data-block/{keyStatisticId}")]
        public async Task<ActionResult<KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
            Guid releaseId,
            Guid keyStatisticId,
            KeyStatisticDataBlockUpdateRequest request)
        {
            return await _keyStatisticService
                .UpdateKeyStatisticDataBlock(releaseId, keyStatisticId, request)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/key-statistic/{keyStatisticId}")]
        public async Task<ActionResult<Unit>> Delete(
            Guid releaseId,
            Guid keyStatisticId)
        {
            return await _keyStatisticService
                .Delete(releaseId, keyStatisticId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/key-statistic/order")]
        public async Task<ActionResult<List<KeyStatisticViewModel>>> ReorderKeyStatistics(
            Guid releaseId,
            Dictionary<Guid, int> newOrder)
        {
            return await _keyStatisticService
                .Reorder(releaseId, newOrder)
                .HandleFailuresOrOk();
        }
    }
}
