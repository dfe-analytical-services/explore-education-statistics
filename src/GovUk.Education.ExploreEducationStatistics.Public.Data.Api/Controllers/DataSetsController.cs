using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
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
    [SwaggerResponse(200, "The requested data set summary", type: typeof(DataSetViewModel))]
    [SwaggerResponse(404)]
    public async Task<ActionResult<DataSetViewModel>> GetDataSet(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId)
    {
        return await _dataSetService
            .GetDataSet(dataSetId)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a data set version
    /// </summary>
    /// <remarks>
    /// Get a data set version's summary details.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/versions/{dataSetVersion}")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The requested data set version", type: typeof(DataSetVersionViewModel))]
    [SwaggerResponse(404)]
    public async Task<ActionResult<DataSetVersionViewModel>> GetVersion(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("The data set version e.g. 1.0, 1.1, 2.0, etc.")] string dataSetVersion)
    {
        return await _dataSetService
            .GetVersion(dataSetId, dataSetVersion)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// List a data set’s versions
    /// </summary>
    /// <remarks>
    /// List a data set’s versions. Only provides summary information of each version.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/versions")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The paginated list of data set versions", type: typeof(DataSetVersionPaginatedListViewModel))]
    [SwaggerResponse(400)]
    public async Task<ActionResult<DataSetVersionPaginatedListViewModel>> ListVersions(
        [FromQuery] DataSetVersionListRequest request,
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId)
    {
        return await _dataSetService
            .ListVersions(
                dataSetId: dataSetId,
                page: request.Page,
                pageSize: request.PageSize)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a data set’s metadata
    /// </summary>
    /// <remarks>
    /// Get the metadata about a data set. Use this to create data set queries.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/meta")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The requested data set version metadata", type: typeof(DataSetMetaViewModel))]
    [SwaggerResponse(403)]
    [SwaggerResponse(404)]
    public async Task<ActionResult<DataSetMetaViewModel>> GetMeta(
        [SwaggerParameter("The version of the data set to use e.g. 2.0, 1.1, etc.")] [FromQuery] string? dataSetVersion,
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId)
    {
        return await _dataSetService
            .GetMeta(dataSetId, dataSetVersion)
            .HandleFailuresOrOk();
    }
}
