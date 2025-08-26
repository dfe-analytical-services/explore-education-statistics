#nullable enable
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

    [HttpPost("release/{releaseVersionId:guid}/key-statistic-data-block")]
    public async Task<ActionResult<KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseVersionId,
        KeyStatisticDataBlockCreateRequest request)
    {
        return await _keyStatisticService
            .CreateKeyStatisticDataBlock(releaseVersionId, request)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/key-statistic-text")]
    public async Task<ActionResult<KeyStatisticTextViewModel>> CreateKeyStatisticText(
        Guid releaseVersionId,
        KeyStatisticTextCreateRequest request)
    {
        return await _keyStatisticService
            .CreateKeyStatisticText(releaseVersionId, request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/key-statistic-data-block/{keyStatisticId:guid}")]
    public async Task<ActionResult<KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request)
    {
        return await _keyStatisticService
            .UpdateKeyStatisticDataBlock(releaseVersionId: releaseVersionId,
                keyStatisticId: keyStatisticId,
                request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/key-statistic-text/{keyStatisticId:guid}")]
    public async Task<ActionResult<KeyStatisticTextViewModel>> UpdateKeyStatisticText(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticTextUpdateRequest request)
    {
        return await _keyStatisticService
            .UpdateKeyStatisticText(releaseVersionId: releaseVersionId,
                keyStatisticId: keyStatisticId,
                request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/key-statistic/{keyStatisticId:guid}")]
    public async Task<ActionResult<Unit>> Delete(
        Guid releaseVersionId,
        Guid keyStatisticId)
    {
        return await _keyStatisticService
            .Delete(releaseVersionId: releaseVersionId,
                keyStatisticId: keyStatisticId)
            .HandleFailuresOrNoContent();
    }

    [HttpPut("release/{releaseVersionId:guid}/key-statistic/order")]
    public async Task<ActionResult<List<KeyStatisticViewModel>>> ReorderKeyStatistics(
        Guid releaseVersionId,
        List<Guid> newOrder)
    {
        return await _keyStatisticService
            .Reorder(releaseVersionId, newOrder)
            .HandleFailuresOrOk();
    }
}
