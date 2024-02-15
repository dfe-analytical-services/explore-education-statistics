using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[ApiController]
[Route("api/v{version:apiVersion}/HelloWorld")]
public class HelloWorldController : ControllerBase
{
    private readonly PublicDataDbContext _publicDataDbContext;

    public HelloWorldController(PublicDataDbContext publicDataDbContext)
    {
        _publicDataDbContext = publicDataDbContext;
    }

    /// <summary>
    /// List v1
    /// </summary>
    /// <remarks>
    /// Test the API can list with `GET` in v1.
    /// </remarks>
    [HttpGet("{dataSetVersionId:guid}")]
    [SwaggerResponse(200, "The list of things", typeof(string))]
    public async Task<ActionResult> List(
        Guid dataSetVersionId,
        [FromQuery(Name = "code1")]
        string code1,
        [FromQuery(Name = "code2")]
        string code2)
    {
        var codes = new HashSet<string> { code1, code2 };

        // var meta = await _publicDataDbContext.LocationMetas
        //     .Where(m => m.Id == metaId)
        //     .FirstAsync();

        var meta = await _publicDataDbContext.LocationMetas
            .AsNoTracking()
            .Where(m => m.DataSetVersionId == dataSetVersionId && (m is LocationSchoolMeta && EF.Functions.JsonContains(
                ((LocationSchoolMeta)m).Options,
                $$"""
                  {"Urn": "102842"}
                  """))
            )
            .AnyAsync();

        return Ok(meta);

        // return Ok();
    }
}
