#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-sets")]
public class DataSetsController(IDataSetService dataSetService) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<PaginatedListViewModel<DataSetSummaryViewModel>>> ListDataSets(
        [FromQuery] DataSetListRequest request,
        CancellationToken cancellationToken
    )
    {
        return await dataSetService
            .ListDataSets(
                page: request.Page,
                pageSize: request.PageSize,
                publicationId: request.PublicationId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    [HttpGet("{dataSetId:guid}")]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetService
            .GetDataSet(dataSetId: dataSetId, cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult<DataSetViewModel>> CreateDataSet(
        [FromBody] DataSetCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await dataSetService
            .CreateDataSet(
                releaseFileId: request.ReleaseFileId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }
}
