#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
    public async Task<ActionResult<PaginatedListViewModel<DataSetFileSummaryViewModel>>> ListDataSetFiles(
        [FromQuery] DataSetFileListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _dataSetFileService
            .ListDataSetFiles(
                themeId: request.ThemeId,
                publicationId: request.PublicationId,
                releaseVersionId: request.ReleaseId,
                geographicLevel: request.GeographicLevelEnum,
                latestOnly: request.LatestOnly,
                dataSetType: request.DataSetType,
                searchTerm: request.SearchTerm,
                sort: request.Sort,
                sortDirection: request.SortDirection,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpGet("data-set-files/{dataSetFileId:guid}")]
    public async Task<ActionResult<DataSetFileViewModel>> GetDataSetFile(
        Guid dataSetFileId)
    {
        return await _dataSetFileService
            .GetDataSetFile(dataSetFileId)
            .HandleFailuresOrOk();
    }

    [HttpGet("data-set-files/{dataSetFileId:guid}/download")] // TODO EES-5979 analytics
    public async Task<ActionResult> DownloadDataSetFile(
        Guid dataSetFileId)
    {
        return await _dataSetFileService
            .DownloadDataSetFile(dataSetFileId);
    }

    [HttpGet("data-set-files/sitemap-items")]
    public async Task<ActionResult<List<DataSetSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default) =>
        await _dataSetFileService.ListSitemapItems(cancellationToken)
            .HandleFailuresOrOk();
}
