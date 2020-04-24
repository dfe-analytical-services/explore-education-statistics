using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU.UserManagement
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "CanManageUsersOnSystem")]
    public class UserInviteController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserInviteController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("bau/users/invite")]
        public async Task<ActionResult<List<UserViewModel>>> GetInvitedUsers()
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
            var invite = await _userManagementService.InviteAsync(userInviteViewModel.Email, User.Identity.Name,
                userInviteViewModel.RoleId);

            if (invite)
            {
                return Ok();
            }

            AddErrors(ModelState, ValidationResult(UserAlreadyExists));

            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }

        [HttpDelete("bau/users/invite/{email}")]
        public async Task<ActionResult> CancelUserInvite(string email)
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