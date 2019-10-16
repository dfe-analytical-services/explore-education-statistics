using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        [HttpGet("api/signout")]
        public IActionResult Signout()
        {
            // TODO - we'll probably need to do something more robust to tell AD that
            // the user is logging out of this particular service - at the moment hitting
            // sign-in would immediately log them back in as AD will recognise that this
            // user has already requested a cookie before and will reissue it without
            // enforcing another login
            
            Response.Cookies.Delete(".AspNetCore.AzureADCookie");
            return Redirect("/signed-out");
        }
        
        [Authorize]
        [HttpGet("api/users/mydetails")]
        public async Task<ActionResult<UserDetailsPermissionsViewModel>> MyDetails()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            // TODO DW - need to store first name and last name in ApplicationUser
            return new UserDetailsPermissionsViewModel()
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email,
                Name = user.NormalizedEmail.Substring(0, user.NormalizedEmail.IndexOf('@')),
                Permissions = roles.ToArray(),
            };
        }
    }
}