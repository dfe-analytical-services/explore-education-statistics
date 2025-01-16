using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class ReleasesController(IReleaseService releaseService) : ControllerBase
{
    [HttpPost("releases")]
    public async Task<ActionResult<ReleaseVersionViewModel>> CreateRelease(ReleaseCreateRequest release)
    {
        return await releaseService
            .CreateRelease(release)
            .HandleFailuresOrOk();
    }
}
