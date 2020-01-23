using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;

        public PermissionsController(
            IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _userService = userService;
            _persistenceHelper = persistenceHelper;
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

        private async Task<ActionResult<bool>> CheckPolicy(params Func<Task<Either<ActionResult, bool>>>[] policyChecks) 
        {
            foreach (var policyCheck in policyChecks)
            {
                var result = await policyCheck();

                if (result.IsRight)
                {
                    return new OkObjectResult(true);
                }
            }

            return new OkObjectResult(false);
        }
    }
}