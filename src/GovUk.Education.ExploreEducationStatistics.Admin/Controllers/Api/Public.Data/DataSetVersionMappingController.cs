#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-versions/{nextDataSetVersionId:guid}/mapping")]
public class DataSetVersionMappingController(IDataSetVersionMappingService mappingService)
    : ControllerBase
{
    [HttpGet("locations")]
    [Produces("application/json")]
    public Task<ActionResult<LocationMappingPlan>> GetLocationMappings(
        [FromRoute] Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        return mappingService
            .GetLocationMappings(nextDataSetVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPatch("locations")]
    [Produces("application/json")]
    public async Task<ActionResult<BatchLocationMappingUpdatesResponseViewModel>> ApplyBatchLocationMappingUpdates(
        [FromRoute] Guid nextDataSetVersionId,
        [FromBody] BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        return await mappingService
            .ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersionId,
                request: request,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpGet("filters")]
    [Produces("application/json")]
    public Task<ActionResult<FilterMappingPlan>> GetFilterMappings(
        [FromRoute] Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        return mappingService
            .GetFilterMappings(nextDataSetVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPatch("filters/options")]
    [Produces("application/json")]
    public async Task<ActionResult<BatchFilterOptionMappingUpdatesResponseViewModel>> ApplyBatchFilterOptionMappingUpdates(
        [FromRoute] Guid nextDataSetVersionId,
        [FromBody] BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        return await mappingService
            .ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersionId,
                request: request,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
