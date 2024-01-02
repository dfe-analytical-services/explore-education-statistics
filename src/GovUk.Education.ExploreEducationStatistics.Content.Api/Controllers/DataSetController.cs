#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class DataSetController
{
    private readonly IDataSetService _dataSetService;

    public DataSetController(IDataSetService dataSetService)
    {
        _dataSetService = dataSetService;
    }

    [HttpGet("data-sets")]
    public async Task<ActionResult<PaginatedListViewModel<DataSetSearchResultViewModel>>> ListDataSets(
        [FromQuery] DataSetListRequest request)
    {
        return await _dataSetService
            .ListDataSets(
                themeId: request.ThemeId,
                publicationId: request.PublicationId,
                releaseId: request.ReleaseId,
                request.SearchTerm,
                request.OrderBy,
                request.SortOrder,
                page: request.Page,
                pageSize: request.PageSize)
            .HandleFailuresOrOk();
    }
}
