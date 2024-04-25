#nullable enable
using System.Threading;
using System.Threading.Tasks;
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
public class DataSetsController(IDataSetService dataSetService) : ControllerBase
{
    [HttpGet("api/public-data/data-sets")]
    [Produces("application/json")]
    public async Task<ActionResult<PaginatedListViewModel<DataSetViewModel>>> ListDataSets(
        [FromQuery] DataSetListRequest request,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .ListDataSets(
                page: request.Page,
                pageSize: request.PageSize,
                publicationId: request.PublicationId,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
