using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Query;
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
    private readonly IDataSetQueryService _dataSetQueryService;

    public DataSetsController(IDataSetService dataSetService, IDataSetQueryService dataSetQueryService)
    {
        _dataSetService = dataSetService;
        _dataSetQueryService = dataSetQueryService;
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
                page: request.Page,
                pageSize: request.PageSize,
                dataSetId: dataSetId)
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
    /// and consequently cannot express more complex queries.
    ///
    /// ### Geographic levels
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
    /// ### Locations
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
    /// #### Examples
    ///
    /// - `LA|code|E08000019` matches any local authority with code `E08000019`
    /// - `REG|id|abcde` matches any region with ID `abcde`
    /// - `SCH|urn|140821` matches any school with URN `140821`
    ///
    /// ### Time periods
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
    ///
    /// #### Examples
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
    /// ## Filters
    ///
    /// The `filters` query parameter is used to filter by other filter options (not locations,
    /// geographic levels or time periods).
    ///
    /// Each filter should be strings containing the required filter option ID.
    /// </remarks>
    [HttpGet("{dataSetId:guid}/query")]
    [Produces("application/json")]
    [SwaggerResponse(200, "The paginated list of query results")]
    [SwaggerResponse(400)]
    [SwaggerResponse(404)]
    public async Task<ActionResult> QueryDataSetGet(
        Guid dataSetId,
        [FromQuery] DataSetGetQueryRequest request)
    {
        return await _dataSetQueryService
            .Query(dataSetId, request.ToCriteria())
            .HandleFailuresOrOk();
    }
}
