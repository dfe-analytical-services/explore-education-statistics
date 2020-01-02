using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize]
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
        
        [HttpPost("bau/users/invite")]
        public async Task<ActionResult> GetUserList(UserInvite userInvite)
        {
            var invite = await _userManagementService.InviteAsync(userInvite.Email);

            if (invite)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}