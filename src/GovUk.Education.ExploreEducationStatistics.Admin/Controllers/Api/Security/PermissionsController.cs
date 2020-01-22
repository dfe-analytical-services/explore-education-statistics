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
        public Task<ActionResult<bool>> CanAccessSystem()
        {
            return CheckPolicy(_userService.CheckCanAccessSystem);
        }

        [HttpGet("administration/access")]
        public Task<ActionResult<bool>> CanAccessAdministration()
        {
            return CheckPolicy(
                _userService.CheckCanManageAllUsers,
                _userService.CheckCanManageAllMethodologies
            );
        }

        [HttpGet("analyst/access")]
        public Task<ActionResult<bool>> CanAccessAnalystPages()
        {
            return CheckPolicy(_userService.CheckCanAccessAnalystPages);
        }

        [HttpGet("prerelease/access")]
        public Task<ActionResult<bool>> CanAccessPrereleasePages()
        {
            return CheckPolicy(_userService.CheckCanAccessPrereleasePages);
        }
        
        [HttpGet("users/manage")]
        public Task<ActionResult<bool>> CanManageAllUsers()
        {
            return CheckPolicy(_userService.CheckCanManageAllUsers);
        }
        
        [HttpGet("methodologies/manage")]
        public Task<ActionResult<bool>> CanManageAllMethodologies()
        {
            return CheckPolicy(_userService.CheckCanManageAllMethodologies);
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