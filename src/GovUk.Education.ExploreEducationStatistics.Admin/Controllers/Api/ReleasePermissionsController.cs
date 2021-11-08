using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasePermissionsController : ControllerBase
    {
        private readonly IReleasePermissionService _releasePermissionService;

        public ReleasePermissionsController(
            IReleasePermissionService releasePermissionService)
        {
            _releasePermissionService = releasePermissionService;
        }

        [HttpGet("releases/{releaseId}/contributors")]
        public async Task<ActionResult<List<ManageAccessPageContributorViewModel>>> GetReleaseContributors(Guid releaseId)
        {
            return await _releasePermissionService
                .GetManageAccessPageContributorList(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
