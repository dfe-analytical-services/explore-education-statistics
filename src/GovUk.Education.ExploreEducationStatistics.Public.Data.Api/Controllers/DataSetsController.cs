using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/data-sets")]
public class DataSetsController : ControllerBase
{
    private readonly IDataSetService _dataSetService;

    public DataSetsController(IDataSetService dataSetService)
    {
        _dataSetService = dataSetService;
    }

    /// <summary>
    /// Get a data set’s summary
    /// </summary>
    /// <remarks>
    /// Gets a specific data set’s summary details.
    /// </remarks>
    [HttpGet("{dataSetId:guid}")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The requested data-set summary", type: typeof(DataSetViewModel))]
    [SwaggerResponse(400)]
    [SwaggerResponse(404)]
    public async Task<ActionResult<DataSetViewModel>> GetDataSet(
        [SwaggerParameter("The ID of the data-set.")] Guid dataSetId)
    {
        return await _dataSetService
            .GetDataSet(dataSetId)
            .HandleFailuresOrOk();
    }
}
