using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        [HttpGet("api/users/mydetails")]
        public async Task<ActionResult<UserDetailsPermissionsViewModel>> MyDetails()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            return new UserDetailsPermissionsViewModel()
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email,
                Name = user.FirstName,
                Permissions = roles.ToArray(),
            };
        }
    }
}