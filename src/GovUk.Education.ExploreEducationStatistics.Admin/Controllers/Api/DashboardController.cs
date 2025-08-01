#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DashboardController(
    IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("dashboard/{slug:string}")]
    public async Task<ActionResult<DashboardViewModel>> GetDashboard(
        [FromRoute] string slug)
    {
        return await dashboardService.GetDashboard(slug)
            .HandleFailuresOrOk();
    }

}
