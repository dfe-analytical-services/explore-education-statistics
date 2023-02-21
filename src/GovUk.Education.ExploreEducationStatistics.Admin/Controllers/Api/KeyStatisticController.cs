#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

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

    [HttpPost("release/{releaseId:guid}/key-statistic-data-block")]
    public async Task<ActionResult<KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseId,
        KeyStatisticDataBlockCreateRequest request)
    {
        return await _keyStatisticService
            .CreateKeyStatisticDataBlock(releaseId, request)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseId:guid}/key-statistic-text")]
    public async Task<ActionResult<KeyStatisticTextViewModel>> CreateKeyStatisticText(
        Guid releaseId,
        KeyStatisticTextCreateRequest request)
    {
        return await _keyStatisticService
            .CreateKeyStatisticText(releaseId, request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseId:guid}/key-statistic-data-block/{keyStatisticId:guid}")]
    public async Task<ActionResult<KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request)
    {
        return await _keyStatisticService
            .UpdateKeyStatisticDataBlock(releaseId, keyStatisticId, request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseId:guid}/key-statistic-text/{keyStatisticId:guid}")]
    public async Task<ActionResult<KeyStatisticTextViewModel>> UpdateKeyStatisticText(
        Guid releaseId,
        Guid keyStatisticId,
        KeyStatisticTextUpdateRequest request)
    {
        return await _keyStatisticService
            .UpdateKeyStatisticText(releaseId, keyStatisticId, request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseId:guid}/key-statistic/{keyStatisticId:guid}")]
    public async Task<ActionResult<Unit>> Delete(
        Guid releaseId,
        Guid keyStatisticId)
    {
        return await _keyStatisticService
            .Delete(releaseId, keyStatisticId)
            .HandleFailuresOrNoContent();
    }

    [HttpPut("release/{releaseId:guid}/key-statistic/order")]
    public async Task<ActionResult<List<KeyStatisticViewModel>>> ReorderKeyStatistics(
        Guid releaseId,
        List<Guid> newOrder)
    {
        return await _keyStatisticService
            .Reorder(releaseId, newOrder)
            .HandleFailuresOrOk();
    }
}
