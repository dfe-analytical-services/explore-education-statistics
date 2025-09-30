using System.Net.Mime;
using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion("1")]
[ApiController]
[Route("v{version:apiVersion}/data-sets")]
public class DataSetsController(
    IDataSetService dataSetService,
    IDataSetQueryService dataSetQueryService)
    : ControllerBase
{
    /// <summary>
    /// Get a data set's summary
    /// </summary>
    /// <remarks>
    /// Gets a data set's summary details.
    /// </remarks>
    [HttpGet("{dataSetId:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The requested data set summary.", type: typeof(DataSetViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetViewModel>> GetDataSet(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .GetDataSet(
               dataSetId: dataSetId,
               cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Get a data set's metadata
    /// </summary>
    /// <remarks>
    /// Get the metadata about a data set. Use this to create data set queries.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/meta")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The requested data set version metadata.", type: typeof(DataSetMetaViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetMetaViewModel>> GetDataSetMeta(
        [FromQuery] DataSetMetaRequest request,
        [FromQuery, SwaggerParameter("""
                                     The data set version e.g. 1.0, 1.1, 2.0, etc.
                                     Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                                     """)] string? dataSetVersion,
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .GetMeta(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                types: request.ParsedTypes(),
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Query a data set (GET)
    /// </summary>
    /// <remarks>
    /// Query a data set using a `GET` request, returning the filtered results.
    ///
    /// Note that there is a `POST` variant of this endpoint which provides a more complete set
    /// of querying functionality. The `GET` variant is only recommended for initial exploratory
    /// testing or simple queries that do not need advanced functionality.
    ///
    /// Unlike the `POST` variant, this endpoint does not allow condition clauses (`and`, `or`, `not`)
    /// and consequently cannot express more complex queries. Use the `POST` variant instead for these
    /// types of queries.
    ///
    /// ## Indicators
    ///
    /// The `indicators` query parameter is used to include only specific values in the data set
    /// results.
    ///
    /// Each indicator should be a string containing the indicator ID e.g. `4xbOu`, `8g1RI`.
    ///
    /// Omitting the `indicators` parameter will return values for all indicators.
    /// 
    /// ## Filters
    ///
    /// The `filters` query parameter is used to filter by other filter options (not locations,
    /// geographic levels or time periods).
    ///
    /// Each filter should be a string containing the required filter option ID e.g. `z4FQE`, `DcQeg`.
    ///
    /// ## Geographic levels
    ///
    /// The `geographicLevels` query parameter is used to filter results by geographic level.
    ///
    /// The geographic levels are specified as codes, and can be one of the following:
    ///
    /// - `EDA` - English devolved area
    /// - `INST` - Institution
    /// - `LA` - Local authority
    /// - `LAD` - Local authority district
    /// - `LEP` - Local enterprise partnership
    /// - `LSIP` - Local skills improvement plan area
    /// - `MCA` - Mayoral combined authority
    /// - `MAT` - MAT
    /// - `NAT` - National
    /// - `OA` - Opportunity area
    /// - `PA` - Planning area
    /// - `PCON` - Parliamentary constituency
    /// - `PROV` - Provider
    /// - `REG` - Regional
    /// - `RSC` - RSC region
    /// - `SCH` - School
    /// - `SPON` - Sponsor
    /// - `WARD` - Ward
    ///
    /// ## Locations
    ///
    /// The `locations` query parameter is used to filter results by location.
    ///
    /// The locations should be strings formatted like `{level}|{property}|{value}` where:
    ///
    /// - `{level}` is the location's level code (e.g. `NAT`, `REG`, `LA`)
    /// - `{property}` is the name of the identifying property to match on (e.g. `id, `code`, `urn`)
    /// - `{value}` is the value for the property to match
    ///
    /// An ID or a code can be used to identify a location, with the following differences:
    ///
    /// - IDs only match a **single location**
    /// - Codes may match **multiple locations**
    ///
    /// Whilst codes are generally unique to a single location, they can be used for multiple locations.
    /// This may match more results than you expect so it's recommended to use IDs where possible.
    ///
    /// ### Examples
    ///
    /// - `LA|code|E08000019` matches any local authority with code `E08000019`
    /// - `REG|id|6bQgZ` matches any region with ID `6bQgZ`
    /// - `SCH|urn|140821` matches any school with URN `140821`
    ///
    /// ## Time periods
    ///
    /// The `timePeriods` query parameter is used to filter results by time period.
    ///
    /// The time periods should be strings formatted like `{period}|{code}` where:
    ///
    /// - `period` is the time period or range (e.g. `2020` or `2020/2021`)
    /// - `code` is the code identifying the time period type (e.g. `AY`, `CY`, `M1`, `W20`)
    ///
    /// The `period` should be a single year like `2020`, or a range like `2020/2021`.
    /// Currently, only years (or year ranges) are supported.
    ///
    /// Some time period types span two years e.g. financial year part 2 (`P2`), or may fall in a
    /// latter year e.g. academic year summer term (`T3`). For these types, a singular year `period`
    /// like `2020` is considered as `2020/2021`.
    ///
    /// For example, a `period` value of `2020` is applicable to the following time periods:
    ///
    /// - 2020 calendar year
    /// - 2020/2021 academic year
    /// - 2020/2021 financial year part 2 (October to March)
    /// - 2020/2021 academic year's summer term
    ///
    /// If you wish to be more explicit, you may use a range for the `period` e.g. `2020/2021`.
    /// However, a range cannot be used with time period types which only span a single year,
    /// for example, `2020/21` cannot be used with `CY`, `M` or `W` codes.
    ///
    /// ### Examples
    ///
    /// - `2020|AY` is the 2020/21 academic year
    /// - `2021|FY` is the 2021/22 financial year
    /// - `2020|T3` is the 2020/21 academic year's summer term
    /// - `2020|P2` is the 2020/21 financial year part 2 (October to March)
    /// - `2020|CY` is the 2020 calendar year
    /// - `2020|W32` is 2020 week 32
    /// - `2020/2021|AY` is the 2020/21 academic year
    /// - `2021/2022|FY` is the 2021/22 financial year
    ///
    /// ## Sorts
    ///
    /// The `sorts` query parameter is used to sort the results.
    ///
    /// Sorts are applied in the order they are provided and should be strings
    /// formatted like `{field}|{direction}` where:
    ///
    /// - `field` is the name of the field to sort e.g. `timePeriod`
    /// - `direction` is the direction to sort in e.g. ascending (`Asc`) or descending (`Desc`)
    ///
    /// The `field` can be one of the following:
    ///
    /// - `timePeriod` to sort by time period
    /// - `geographicLevel` to sort by the geographic level of the data
    /// - `location|{level}` to sort by locations in a geographic level where `{level}` is the level code (e.g. `REG`, `LA`)
    /// - `filter|{id}` to sort by the options in a filter where `{id}` is the filter ID (e.g. `3RxWP`)
    /// - `indicator|{id}` to sort by the values in a indicator where `{id}` is the indicator ID (e.g. `6VfPgZ`)
    ///
    /// ### Examples
    ///
    /// - `timePeriod|Desc` sorts by time period in descending order
    /// - `geographicLevel|Asc` sorts by geographic level in ascending order
    /// - `location|REG|Asc` sorts by regions in ascending order
    /// - `filter|3RxWP|Desc` sorts by options in filter `3RxWP` in descending order
    /// - `indicator|7a1dk|Asc` sorts by values in indicator `7a1dk` in ascending order
    /// </remarks>
    [HttpGet("{dataSetId:guid}/query")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The paginated list of query results.", type: typeof(DataSetQueryPaginatedResultsViewModel))]
    [SwaggerResponse(400, type: typeof(ValidationProblemViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetQueryPaginatedResultsViewModel>> QueryDataSetGet(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("""
                          The data set version e.g. 1.0, 1.1, 2.0, etc.
                          Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                          """)][FromQuery] string? dataSetVersion,
        [FromQuery] DataSetGetQueryRequest request,
        CancellationToken cancellationToken)
    {
        return await dataSetQueryService
            .Query(
                dataSetId: dataSetId,
                request: request,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Query a data set (POST)
    /// </summary>
    /// <remarks>
    /// Query a data set using a `POST` request, returning the filtered results.
    ///
    /// Note that for simpler queries or exploratory testing, there is also GET variant of this endpoint
    /// only handles a smaller subset of querying functionality. However, for most use-cases,
    /// this endpoint is recommended as it provides the complete set of functionality.
    ///
    /// Unlike the `GET` endpoint, the `POST` endpoint allows condition criteria (`and`, `or`, `not`)
    /// and consequently can express more complex queries.
    ///
    /// A `POST` request without a body will return a paginated set of unfiltered results and will include
    /// values for all indicators.
    ///
    /// A `Content-Type` request header of `application/json` is required.
    /// </remarks>
    [HttpPost("{dataSetId:guid}/query")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerResponse(200, "The paginated list of query results.", type: typeof(DataSetQueryPaginatedResultsViewModel))]
    [SwaggerResponse(400, type: typeof(ValidationProblemViewModel))]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel))]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel))]
    public async Task<ActionResult<DataSetQueryPaginatedResultsViewModel>> QueryDataSetPost(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("""
                          The data set version e.g. 1.0, 1.1, 2.0, etc.
                          Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                          """)][FromQuery] string? dataSetVersion,
        [FromBody] DataSetQueryRequest? request,
        CancellationToken cancellationToken)
    {
        return await dataSetQueryService
            .Query(
                dataSetId: dataSetId,
                request: request ?? new DataSetQueryRequest(),
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }

    /// <summary>
    /// Download a data set as CSV
    /// </summary>
    /// <remarks>
    /// The CSV response will render its metadata in a human-readable format (instead of
    /// machine-readable IDs). The CSV is not subject to the same backward compatibility
    /// guarantees as the data set's JSON representation in other endpoints.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/csv")]
    [SwaggerResponse(
        200,
        description: "The data set CSV file.",
        type: typeof(string),
        contentTypes: MediaTypeNames.Text.Csv
    )]
    [SwaggerResponse(403, type: typeof(ProblemDetailsViewModel), contentTypes: MediaTypeNames.Application.Json)]
    [SwaggerResponse(404, type: typeof(ProblemDetailsViewModel), contentTypes: MediaTypeNames.Application.Json)]
    public async Task<ActionResult> DownloadDataSetCsv(
        [SwaggerParameter("The ID of the data set.")] Guid dataSetId,
        [SwaggerParameter("""
                          The data set version e.g. 1.0, 1.1, 2.0, etc.
                          Wildcard versions are supported. For example, `2.*` returns the latest minor version in the v2 series.
                          """)][FromQuery] string? dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await dataSetService
            .DownloadDataSet(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken
            )
            .OnSuccess(fileStreamResult =>
            {
                HttpContext.Response.Headers["X-Robots-Tag"] = "noindex";
                HttpContext.Response.Headers.ContentEncoding = ContentEncodings.Gzip;

                return fileStreamResult;
            })
            .HandleFailures();
    }
}
