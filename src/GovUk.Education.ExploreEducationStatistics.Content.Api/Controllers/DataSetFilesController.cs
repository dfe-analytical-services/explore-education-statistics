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
public class DataSetFilesController : ControllerBase
{
    private readonly IDataSetFileService _dataSetFileService;

    public DataSetFilesController(IDataSetFileService dataSetFileService)
    {
        _dataSetFileService = dataSetFileService;
    }

    [HttpGet("data-set-files")]
    [MemoryCache(typeof(ListDataSetFilesCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
    public async Task<ActionResult<PaginatedListViewModel<DataSetFileSummaryViewModel>>> ListDataSets(
        [FromQuery] DataSetFileListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _dataSetFileService
            .ListDataSetFiles(
                themeId: request.ThemeId,
                publicationId: request.PublicationId,
                releaseVersionId: request.ReleaseId,
                request.LatestOnly,
                request.SearchTerm,
                request.Sort,
                request.SortDirection,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpGet("data-set-files/{dataSetFileId:guid}")]
    public async Task<ActionResult<DataSetFileViewModel>> GetDataSet(
        Guid dataSetFileId)
    {
        return await _dataSetFileService
            .GetDataSetFile(dataSetFileId)
            .HandleFailuresOrOk();
    }
}