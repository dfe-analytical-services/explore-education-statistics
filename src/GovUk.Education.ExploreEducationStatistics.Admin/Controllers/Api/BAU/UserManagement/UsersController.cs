using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "CanManageUsersOnSystem")]
    public class BauUsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public BauUsersController(IUserManagementService userManagementService)
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
                return Ok(user);
            }
            
            AddErrors(ModelState, ValidationResult(UserDoesNotExist));
            
            return ValidationProblem(new ValidationProblemDetails(ModelState));
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
        public async Task<ActionResult<List<RoleViewModel>>> GetRoleList()
        {
            var users = await _userManagementService.ListRolesAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }
        
        /// <summary>
        /// Provides a list of release roles that are avaliable within the service
        /// </summary>
        /// <returns>Name and value representation of the enum</returns>
        [HttpGet("bau/users/release-roles")]
        public ActionResult<List<EnumExtensions.EnumValue>> GetReleaseRolesList()
        {
            var values = EnumExtensions.GetValues<ReleaseRole>();;

            if (values.Any())
            {
                return Ok(values);
            }

            return NotFound();
        }
        
        
        
    }
}