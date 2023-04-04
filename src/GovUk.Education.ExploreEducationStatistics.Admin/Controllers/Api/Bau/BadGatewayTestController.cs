using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// TODO EES-4218 Temporary controller intended to test the timeout duration between IIS and Kestrel
/// in Azure App Service environments. Remove after testing is completed.
/// </summary>
[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class BadGatewayTestController
{
    [HttpGet("bau/delay")]
    public async Task<ActionResult> Delay([FromQuery] [Range(1, 600)] [Required] int durationSeconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(durationSeconds));
        return new OkResult();
    }
}
