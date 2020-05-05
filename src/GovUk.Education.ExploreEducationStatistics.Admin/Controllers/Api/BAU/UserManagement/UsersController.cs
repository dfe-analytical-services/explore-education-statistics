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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU.UserManagement
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

        [HttpGet("bau/users")]
        public async Task<ActionResult<List<UserViewModel>>> GetUserList()
        {
            var users = await _userManagementService.ListAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }

        [HttpGet("bau/users/{userId}")]
        public async Task<ActionResult<UserViewModel>> GetUser(string userId)
        {
            var user = await _userManagementService.GetAsync(userId);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [HttpPut("bau/users/{userId}")]
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
        
        [HttpPost("bau/users/{userId}/release-role")]
        public async Task<ActionResult<bool>> AddUserReleaseRole(Guid userId, UserReleaseRoleSubmission releaseRole)
        {
            return await _userManagementService.AddUserReleaseRole(userId, releaseRole).HandleFailuresOrOk();
        }
        
        [HttpDelete("bau/users/{userId}/release-role/{userReleaseRoleId}")]
        public async Task<ActionResult<bool>> DeleteUserReleaseRole(Guid userId, Guid userReleaseRoleId)
        {
            return await _userManagementService.RemoveUserReleaseRole(userId, userReleaseRoleId).HandleFailuresOrOk();
        }


        [HttpGet("bau/users/pre-release")]
        public async Task<ActionResult<List<UserViewModel>>> GetPreReleaseUserList()
        {
            var users = await _userManagementService.ListPreReleaseUsersAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }

        [HttpGet("bau/users/roles")]
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
        /// <returns>Name and value representation of the enum</returns>
        [HttpGet("bau/users/release-roles")]
        [ProducesResponseType(200)]
        public List<EnumExtensions.EnumValue> GetReleaseRoles()
        {
            return EnumExtensions.GetValues<ReleaseRole>();
        }
    }
}