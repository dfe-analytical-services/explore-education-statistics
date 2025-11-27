#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
public class DataSetFilesController(
    IDataSetFileService dataSetFileService,
    IMemoryCacheService memoryCacheService,
    ILogger<DataSetFilesController> logger,
    DateTimeProvider dateTimeProvider
) : ControllerBase
{
    [HttpGet("data-set-files")]
    public Task<ActionResult<PaginatedListViewModel<DataSetFileSummaryViewModel>>> ListDataSetFiles(
        [FromQuery] DataSetFileListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return memoryCacheService.GetOrCreateAsync(
            cacheKey: new ListDataSetFilesCacheKey(request),
            createIfNotExistsFn: () =>
                dataSetFileService
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
                        cancellationToken: cancellationToken
                    )
                    .HandleFailuresOrOk(),
            durationInSeconds: 10,
            expiryScheduleCron: HalfHourlyExpirySchedule,
            dateTimeProvider: dateTimeProvider,
            logger: logger
        );
    }

    [HttpGet("data-set-files/{dataSetFileId:guid}")]
    public async Task<ActionResult<DataSetFileViewModel>> GetDataSetFile(
        Guid dataSetFileId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetFileService.GetDataSetFile(dataSetFileId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpGet("data-set-files/{dataSetFileId:guid}/download")]
    public async Task<ActionResult> DownloadDataSetFile(Guid dataSetFileId, CancellationToken cancellationToken)
    {
        HttpContext.Response.Headers["X-Robots-Tag"] = "noindex";

        return await dataSetFileService.DownloadDataSetFile(dataSetFileId, cancellationToken);
    }

    [HttpGet("data-set-files/sitemap-items")]
    public async Task<ActionResult<List<DataSetSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default
    ) => await dataSetFileService.ListSitemapItems(cancellationToken).HandleFailuresOrOk();
}
