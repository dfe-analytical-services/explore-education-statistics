using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IPreReleaseService _preReleaseService;

        public PermissionsController(
            IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IPreReleaseService preReleaseService)
        {
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _preReleaseService = preReleaseService;
        }

        [HttpGet("access")]
        public ActionResult<GlobalPermissions> CanAccessSystem()
        {
            return new GlobalPermissions
            {
                CanAccessSystem = _userService.CheckCanAccessSystem().Result.IsRight,
                CanAccessAnalystPages = _userService.CheckCanAccessAnalystPages().Result.IsRight,
                CanAccessPrereleasePages = _userService.CheckCanAccessPrereleasePages().Result.IsRight,
                CanAccessUserAdministrationPages = _userService.CheckCanManageAllUsers().Result.IsRight,
                CanAccessMethodologyAdministrationPages = _userService.CheckCanManageAllMethodologies().Result.IsRight
            };
        }

        [HttpGet("topic/{topicId}/publication/create")]
        public Task<ActionResult<bool>> CanCreatePublicationForTopic(Guid topicId)
        {
            return CheckPolicyAgainstEntity<Topic>(topicId, _userService.CheckCanCreatePublicationForTopic);
        }

        [HttpGet("publication/{publicationId}/release/create")]
        public Task<ActionResult<bool>> CanCreateReleaseForPublication(Guid publicationId)
        {
            return CheckPolicyAgainstEntity<Publication>(publicationId, _userService.CheckCanCreateReleaseForPublication);
        }

        [HttpGet("release/{releaseId}/update")]
        public Task<ActionResult<bool>> CanUpdateRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanUpdateRelease);
        }
        
        // BAU-324 - temporary stop-gap to prevent data files from being amended during a Release Amendment.
        // Allowing data files to be changed on a Release Amendment is being tackled in the follow-up "Phase 2"
        // Release Versioning tickets
        [HttpGet("release/{releaseId}/update-data-files")]
        public Task<ActionResult<bool>> CanUpdateReleaseDataFiles(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, async release =>
            {
                if (release.Version > 0)
                {
                    return Ok(false);
                }

                return await _userService.CheckCanUpdateRelease(release);
            });
        }

        [HttpGet("release/{releaseId}/status/draft")]
        public Task<ActionResult<bool>> CanMarkReleaseAsDraft(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanMarkReleaseAsDraft);
        }

        [HttpGet("release/{releaseId}/status/submit")]
        public Task<ActionResult<bool>> CanSubmitReleaseToHigherReview(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanSubmitReleaseToHigherApproval);
        }

        [HttpGet("release/{releaseId}/status/approve")]
        public Task<ActionResult<bool>> CanApproveRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanApproveRelease);
        }

        [HttpGet("release/{releaseId}/amend")]
        public Task<ActionResult<bool>> CanMakeAmendmentOfRelease(Guid releaseId)
        {
            return CheckPolicyAgainstEntity<Release>(releaseId, _userService.CheckCanMakeAmendmentOfRelease);
        }

        [HttpGet("release/{releaseId}/prerelease/status")]
        public async Task<ActionResult<PreReleaseWindowStatus>> GetPreReleaseWindowStatus(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => _preReleaseService.GetPreReleaseWindowStatus(release, UtcNow))
                .HandleFailuresOr(Ok);
        }
        
        [HttpGet("methodology/{methodologyId}/update")]
        public Task<ActionResult<bool>> CanUpdateMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanUpdateMethodology);
        }

        [HttpGet("methodology/{methodologyId}/status/draft")]
        public Task<ActionResult<bool>> CanMarkMethodologyAsDraft(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanMarkMethodologyAsDraft);
        }

        [HttpGet("methodology/{methodologyId}/status/approve")]
        public Task<ActionResult<bool>> CanApproveMethodology(Guid methodologyId)
        {
            return CheckPolicyAgainstEntity<Methodology>(methodologyId, _userService.CheckCanApproveMethodology);
        }

        public class GlobalPermissions
        {
            public bool CanAccessSystem { get; set; }
            public bool CanAccessPrereleasePages { get; set; }
            public bool CanAccessAnalystPages { get; set; }
            public bool CanAccessUserAdministrationPages { get; set; }
            public bool CanAccessMethodologyAdministrationPages { get; set; }
        }

        private async Task<ActionResult<bool>> CheckPolicyAgainstEntity<TEntity>(
            Guid entityId,
            Func<TEntity, Task<Either<ActionResult, TEntity>>> policyCheck)
            where TEntity : class
        {
            return await _persistenceHelper
                .CheckEntityExists<TEntity, Guid>(entityId)
                .OnSuccess(policyCheck.Invoke)
                .OnSuccess(_ => new OkObjectResult(true))
                .OrElse(() => new OkObjectResult(false));
        }
    }
}