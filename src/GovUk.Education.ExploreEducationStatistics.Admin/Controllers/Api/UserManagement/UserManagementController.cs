#nullable enable
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
    [ApiController]
    [Route("api/user-management")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IUserRoleService _userRoleService;

        public UserManagementController(IUserManagementService userManagementService,
            IUserRoleService userRoleService)
        {
            _userManagementService = userManagementService;
            _userRoleService = userRoleService;
        }

        [HttpGet("users")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<UserViewModel>>> GetUserList()
        {
            return await _userManagementService
                .ListAllUsers()
                .HandleFailuresOrOk();
        }

        [HttpGet("users/{id:guid}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<UserViewModel>> GetUser(Guid id)
        {
            return await _userManagementService
                .GetUser(id)
                .HandleFailuresOrOk();
        }

        [HttpPut("users/{userId:guid}")]
        public async Task<ActionResult<Unit>> UpdateUser(Guid userId, UserEditViewModel model)
        {
            return await _userManagementService
                .UpdateUser(userId.ToString(), model.RoleId)
                .HandleFailuresOrOk();
        }

        [HttpPost("users/{userId:guid}/publication-role")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> AddPublicationRole(Guid userId, UserPublicationRoleCreateRequest request)
        {
            return await _userRoleService
                .AddPublicationRole(userId, request.PublicationId, request.PublicationRole)
                .HandleFailuresOrOk();
        }

        [HttpPost("users/{userId:guid}/release-role")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> AddReleaseRole(Guid userId, UserReleaseRoleCreateRequest request)
        {
            return await _userRoleService
                .AddReleaseRole(userId, request.ReleaseId, request.ReleaseRole)
                .HandleFailuresOrOk();
        }

        [HttpDelete("users/publication-role/{id:guid}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> DeleteUserPublicationRole(Guid id)
        {
            return await _userRoleService
                .RemoveUserPublicationRole(id)
                .HandleFailuresOrOk();
        }

        [HttpDelete("users/release-role/{id:guid}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Unit>> DeleteUserReleaseRole(Guid id)
        {
            return await _userRoleService
                .RemoveUserReleaseRole(id)
                .HandleFailuresOrOk();
        }

        /// <summary>
        ///   Removes all user resource roles for the given user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("user/{userId:guid}/resource-roles/all")] [ProducesResponseType(204)]
        public async Task<ActionResult<Unit>> DeleteAllUserResourceRoles(Guid userId)
        {
            return await _userRoleService
                .RemoveAllUserResourceRoles(userId)
                .HandleFailuresOrNoContent();
        }
        
        /// <summary>
        /// Provides a list of releases that are available within the service
        /// </summary>
        /// <returns>Id and Title of the releases</returns>
        [HttpGet("releases")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<IdTitleViewModel>>> GetReleases()
        {
            return await _userManagementService
                .ListReleases()
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of global roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of role</returns>
        [HttpGet("roles")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<List<RoleViewModel>>> GetGlobalRoles()
        {
            return await _userRoleService
                .GetAllGlobalRoles()
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Provides a list of resource roles that are available within the service
        /// </summary>
        /// <returns>Name and value representation of the resource role</returns>
        [HttpGet("resource-roles")]
        [ProducesResponseType(200)]
        public Task<ActionResult<Dictionary<string, List<string>>>> GetResourceRoles()
        {
            return _userRoleService
                .GetAllResourceRoles()
                .HandleFailuresOrOk();
        }
    }
}