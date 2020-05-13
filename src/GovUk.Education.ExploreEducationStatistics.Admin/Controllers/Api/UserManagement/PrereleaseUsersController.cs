using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "CanManageUsersOnSystem")]
    public class PrereleaseUsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public PrereleaseUsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("user-management/pre-release")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<UserViewModel>>> GetPreReleaseUserList()
        {
            var users = await _userManagementService.ListPreReleaseUsersAsync();

            if (users.Any())
            {
                return Ok(users);
            }

            return NotFound();
        }
    }
}