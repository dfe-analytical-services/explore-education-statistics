using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper;
        private readonly IUserService _userService;

        public PermissionsController(IUserService userService, IPersistenceHelper<Release, Guid> releaseHelper)
        {
            _userService = userService;
            _releaseHelper = releaseHelper;
        }

        [HttpGet("release/{releaseId}/update")]
        public Task<ActionResult<bool>> CanUpdateRelease(Guid releaseId)
        {
            return CheckPolicyAgainstRelease(releaseId, _userService.CheckCanUpdateRelease);
        }

        [HttpGet("release/{releaseId}/status/submit")]
        public Task<ActionResult<bool>> CanSubmitReleaseToHigherReview(Guid releaseId)
        {
            return CheckPolicyAgainstRelease(releaseId, _userService.CheckCanSubmitReleaseToHigherApproval);
        }

        [HttpGet("release/{releaseId}/status/approve")]
        public Task<ActionResult<bool>> CanApproveRelease(Guid releaseId)
        {
            return CheckPolicyAgainstRelease(releaseId, _userService.CheckCanApproveRelease);
        }

        private async Task<ActionResult<bool>> CheckPolicyAgainstRelease(Guid releaseId, Func<Release, Task<Either<ActionResult, Release>>> policyCheck)
        {
            return await _releaseHelper
                .CheckEntityExists(releaseId)
                .OnSuccess(policyCheck.Invoke)
                .OnSuccess(_ => new OkObjectResult(true))
                .OrElse(() => new OkObjectResult(false));
        }
    }
}