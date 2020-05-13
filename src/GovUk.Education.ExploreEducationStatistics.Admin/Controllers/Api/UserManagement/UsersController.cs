using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "CanManageUsersOnSystem")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("user-management/users")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<UserViewModel>>> GetUserList()
        {
            var users = await _userManagementService.ListAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }

        [HttpGet("user-management/users/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserViewModel>> GetUser(string userId)
        {
            var user = await _userManagementService.GetAsync(userId);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [HttpPut("user-management/users/{userId}")]
        public async Task<ActionResult<UserViewModel>> UpdateUser(string userId, EditUserViewModel model)
        {
            var user = await _userManagementService.UpdateAsync(userId, model.RoleId);

            if (user)
            {
                return Ok(true);
            }

            AddErrors(ModelState, ValidationResult(UserDoesNotExist));

            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }

        [HttpGet("user-management/users/{userId}/release-role")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<List<UserReleaseRoleViewModel>> GetUserReleaseRoles(Guid userId)
        {
            var userReleaseRoles = _userManagementService.GetUserReleaseRoles(userId.ToString());

            if (userReleaseRoles.Any())
            {
                return Ok(userReleaseRoles);
            }

            return NotFound();
        }

        [HttpPost("user-management/users/{userId}/release-role")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<UserReleaseRoleViewModel>> AddUserReleaseRole(Guid userId,
            UserReleaseRoleSubmission releaseRole)
        {
            return await _userManagementService.AddUserReleaseRole(userId, releaseRole).HandleFailuresOrOk();
        }

        [HttpGet("user-management/users/{userId}/release-role/{userReleaseRoleId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserReleaseRoleViewModel>> GetUserReleaseRole(Guid userId,
            Guid userReleaseRoleId)
        {
            var userReleaseRole = await _userManagementService.GetUserReleaseRole(userId, userReleaseRoleId);

            if (userReleaseRole != null)
            {
                return Ok(userReleaseRole);
            }

            return NotFound();
        }

        [HttpDelete("user-management/users/{userId}/release-role/{userReleaseRoleId}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<bool>> DeleteUserReleaseRole(Guid userId, Guid userReleaseRoleId)
        {
            return await _userManagementService.RemoveUserReleaseRole(userId, userReleaseRoleId).HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of releases that are available within the service
        /// </summary>
        /// <returns>Id and Title of the release</returns>
        [HttpGet("user-management/releases")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IdTitlePair>> GetReleases()
        {
            var releases = await _userManagementService.ListReleasesAsync();

            if (releases.Any())
            {
                return Ok(releases);
            }

            return NotFound();
        }

        /// <summary>
        /// Provides a list of release roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of role</returns>
        [HttpGet("user-management/roles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<RoleViewModel>>> GetRoles()
        {
            var users = await _userManagementService.ListRolesAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }

        /// <summary>
        /// Provides a list of release roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of the release role</returns>
        [HttpGet("user-management/release-roles")]
        [ProducesResponseType(200)]
        public List<EnumExtensions.EnumValue> GetReleaseRoles()
        {
            return EnumExtensions.GetValues<ReleaseRole>();
        }
    }
}