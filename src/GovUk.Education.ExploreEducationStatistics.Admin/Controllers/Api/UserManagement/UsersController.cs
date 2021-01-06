using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<UserViewModel>>> GetUserList()
        {
            return await _userManagementService
                .ListAllUsers()
                .HandleFailuresOrOk();
        }

        [HttpGet("user-management/users/{userId}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<UserViewModel>> GetUser(string userId)
        {
            return await _userManagementService
                .GetUser(userId)
                .HandleFailuresOrOk();
        }

        [HttpPut("user-management/users/{userId}")]
        public async Task<ActionResult<Unit>> UpdateUser(string userId, EditUserViewModel model)
        {
            return await _userManagementService
                .UpdateUser(userId, model.RoleId)
                .HandleFailuresOrOk();
        }

        [HttpPost("user-management/users/{userId}/release-role")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> AddUserReleaseRole(Guid userId,
            UserReleaseRoleRequest releaseRole)
        {
            return await _userManagementService
                .AddUserReleaseRole(userId, releaseRole)
                .HandleFailuresOrOk();
        }

        [HttpDelete("user-management/users/release-role/{userReleaseRoleId}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> DeleteUserReleaseRole(Guid userReleaseRoleId)
        {
            return await _userManagementService
                .RemoveUserReleaseRole(userReleaseRoleId)
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of releases that are available within the service
        /// </summary>
        /// <returns>Id and Title of the release</returns>
        [HttpGet("user-management/releases")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<IdTitlePair>>> GetReleases()
        {
            return await _userManagementService
                .ListReleases()
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of release roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of role</returns>
        [HttpGet("user-management/roles")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<RoleViewModel>>> GetRoles()
        {
            return await _userManagementService
                .ListRoles()
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of release roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of the release role</returns>
        [HttpGet("user-management/release-roles")]
        [ProducesResponseType(200)]
        public Task<ActionResult<List<EnumExtensions.EnumValue>>> GetReleaseRoles()
        {
            return _userManagementService
                .ListReleaseRoles()
                .HandleFailuresOrOk();
        }
    }
}
