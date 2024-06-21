using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class BoundaryDataController : ControllerBase
{
    private readonly IBoundaryDataService _boundaryDataService;
    private readonly IBoundaryLevelService _boundaryLevelService;

    public BoundaryDataController(
        IBoundaryDataService boundaryDataService,
        IBoundaryLevelService boundaryLevelService)
    {
        _boundaryDataService = boundaryDataService;
        _boundaryLevelService = boundaryLevelService;
    }

    [HttpGet("boundary-data/levels")]
    public async Task<ActionResult<List<BoundaryLevelViewModel>>> GetBoundaryLevels()
    {
        var levels = await _boundaryLevelService.Get();

        return Ok(levels);
    }

    [HttpGet("boundary-data/levels/{id}")]
    public async Task<ActionResult<List<BoundaryLevelViewModel>>> GetBoundaryLevel(long id)
    {
        if (id == 0)
        {
            return BadRequest("Id is required");
        }

        var level = await _boundaryLevelService.Get(id);

        return Ok(level);
    }

    [HttpPatch("boundary-data/levels")]
    public async Task<ActionResult> UpdateBoundaryLevel(BoundaryLevelUpdateRequest updateRequest)
    {
        await _boundaryLevelService.UpdateLabel(updateRequest.Id, updateRequest.Label);

        return NoContent();
    }

    [HttpPost("boundary-data")]
    public async Task<ActionResult> UploadBoundaryFile(
        [FromForm] GeographicLevel level,
        [FromForm] string label,
        IFormFile file,
        [FromForm] DateTime published)
    {
        if (file.FileName.Split('.')[1] != "geojson")
        {
            return BadRequest("Invalid file format. This function only accepts GeoJSON files");
        }

        var existingLevels = await _boundaryLevelService.Get();
        var existingLabels = existingLevels.Select(el => el.Label).ToList();

        if (existingLabels.Contains(label))
        {
            return Conflict($"A boundary level matching \"{label}\" already exists");
        }

        string fileContents;
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            fileContents = await reader.ReadToEndAsync();
        }

        var parsedGeoJson = JsonConvert.DeserializeObject<FeatureCollection>(fileContents, new JsonSerializerSettings { CheckAdditionalContent = false });

        await _boundaryDataService.ProcessGeoJson(level, label, published, parsedGeoJson);

        return NoContent();
    }
}
