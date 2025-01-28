using System.Net.Mime;
using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/data-sets/{dataSetId:guid}/versions")]
public class DataSetVersionsController(
    IDataSetService dataSetService,
    IDataSetVersionChangeService dataSetVersionChangeService)
    : ControllerBase
{
    /// <summary>
    /// List a data set’s versions
    /// </summary>
    /// <remarks>
    /// List a data set’s versions. Only provides summary information of each version.
    /// </remarks>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The paginated list of data set versions.", type: typeof(DataSetVersionPaginatedListViewModel))]
    [SwaggerResponse(400, type: typeof(ValidationProblemViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetVersionPaginatedListViewModel>> ListDataSetVersions(
        [FromQuery] DataSetVersionListRequest request,
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .ListVersions(
                dataSetId: dataSetId,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a data set version
    /// </summary>
    /// <remarks>
    /// Get a data set version's summary details.
    /// </remarks>
    [HttpGet("{dataSetVersion}")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The requested data set version.", type: typeof(DataSetVersionViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetVersionViewModel>> GetDataSetVersion(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("""
                          The data set version e.g. 1.0, 1.1, 2.0, etc.
                          Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                          """)] string dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .GetVersion(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a data set version's changes
    /// </summary>
    /// <remarks>
    /// Lists the changes made by a data set version relative to the version prior to it.
    /// </remarks>
    [HttpGet("{dataSetVersion}/changes")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The changes for the data set version.", type: typeof(DataSetVersionChangesViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetVersionChangesViewModel>> GetDataSetVersionChanges(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("""
                          The data set version e.g. 1.0, 1.1, 2.0, etc.
                          Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                          """)] string dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await dataSetVersionChangeService
            .GetChanges(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}
