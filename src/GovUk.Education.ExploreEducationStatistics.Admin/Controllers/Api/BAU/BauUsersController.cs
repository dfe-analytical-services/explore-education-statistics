using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
        
        [HttpGet("bau/users/pending")]
        public async Task<ActionResult<List<UserViewModel>>> GetPendingUsers()
        {
            var users = await _userManagementService.ListPendingAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }
        
        [HttpPost("bau/users/invite")]
        public async Task<ActionResult> InviteUser(UserInviteViewModel userInviteViewModel)
        {
            var invite = await _userManagementService.InviteAsync(userInviteViewModel.Email, User.Identity.Name, userInviteViewModel.RoleId);

            if (invite)
            {
                return Ok();
            }

            AddErrors(ModelState, ValidationResult(UserAlreadyExists));
            
            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }
        
        [HttpDelete("bau/users/invite/{email}")]
        public async Task<ActionResult> InviteUser(string email)
        {
            var invite = await _userManagementService.CancelInviteAsync(email);

            if (invite)
            {
                return Ok();
            }

            AddErrors(ModelState, ValidationResult(UnableToCancelInvite));
            
            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }
    }
}