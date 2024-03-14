#nullable enable
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class DataSetsController : ControllerBase
{
    private readonly IDataSetService _dataSetService;

    public DataSetsController(IDataSetService dataSetService)
    {
        _dataSetService = dataSetService;
    }

    [HttpGet("data-sets")]
    [MemoryCache(typeof(ListDataSetsCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
    public async Task<ActionResult<PaginatedListViewModel<DataSetListViewModel>>> ListDataSets(
        [FromQuery] DataSetsListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _dataSetService
            .ListDataSets(
                themeId: request.ThemeId,
                publicationId: request.PublicationId,
                releaseVersionId: request.ReleaseId,
                request.LatestOnly,
                request.SearchTerm,
                request.OrderBy,
                request.Sort,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/data-sets/{fileId:guid}")]
    public async Task<ActionResult<DataSetDetailsViewModel>> GetDataSet(
        Guid releaseVersionId,
        Guid fileId)
    {
        return await _dataSetService
            .GetDataSet(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOrOk();
    }
}
