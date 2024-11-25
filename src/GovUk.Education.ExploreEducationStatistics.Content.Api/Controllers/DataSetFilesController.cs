#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[ApiController]
[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class DataSetFilesController : ControllerBase
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IDataSetFileService _dataSetFileService;

    public DataSetFilesController(
        ContentDbContext contentDbContext,
        IDataSetFileService dataSetFileService)
    {
        _contentDbContext = contentDbContext;
        _dataSetFileService = dataSetFileService;
    }

    [HttpGet("data-set-meta-test")] // @MarkFix
    public async Task<List<File>> ListDataSetFiles()
    {
        var files = await _contentDbContext.Files
            .Where(f => f.Type == FileType.Data)
            .ToListAsync();
        foreach (var file in files)
        {
            file.DataSetFileMetaNew = new DataSetFileMetaNew
            {
                //GeographicLevels = ["test", "test"],
                //GeographicLevels = [GeographicLevel.Country, GeographicLevel.LocalAuthority],
                GeographicLevels = [new GeographicLevelMeta { Code = GeographicLevel.Country}, new GeographicLevelMeta { Code = GeographicLevel.LocalAuthority}],
                TimePeriodRange = new TimePeriodRangeMetaNew
                {
                    Start = new TimePeriodRangeBoundMetaNew
                    {
                        Period = "2000",
                        TimeIdentifier = TimeIdentifier.April,
                    },
                    End = new TimePeriodRangeBoundMetaNew
                    {
                        Period = "2001",
                        TimeIdentifier = TimeIdentifier.May,
                    }
                },
                Filters = new List<FilterMetaNew>
                {
                    new FilterMetaNew
                    {
                        FilterId = Guid.NewGuid(),
                        Label = "label",
                        ColumnName = "column_name",
                        Hint = "hint",
                    },
                },
                Indicators = new List<IndicatorMetaNew>
                {
                    new IndicatorMetaNew
                    {
                        IndicatorId = Guid.NewGuid(),
                        Label = "label",
                        ColumnName = "column_name",
                    },
                },
            };
        }

        await _contentDbContext.SaveChangesAsync();

        var countryFiles = _contentDbContext.Files
            .Where(f => f.DataSetFileMetaNew!.GeographicLevels.Any(gl => gl.Code == GeographicLevel.Institution))
            .ToList();

        return countryFiles;
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

    [HttpGet("data-set-files/{dataSetFileId:guid}/download")]
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
