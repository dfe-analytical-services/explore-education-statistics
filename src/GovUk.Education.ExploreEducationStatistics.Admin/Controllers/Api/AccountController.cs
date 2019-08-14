using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        [HttpGet("api/signin")]
        public IActionResult Signin()
        {
            return Redirect("/");
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
        
        [AllowAnonymous]
        [HttpGet("api/users/mydetails")]
        public ActionResult<UserDetailsViewModel> MyDetails()
        {
            // TODO - we need to something cleverer here - we need to 
            // validate that their AD cookie is still valid, not simply
            // to check for its existence.  However, this is just in here
            // temporarily as a stopgap
            if (User.Identity.IsAuthenticated)
            {
                var email = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var givenname = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
                var surname = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;

                // workaround for email being different in azure ad claims
                if (string.IsNullOrWhiteSpace(email))
                {
                    email = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
                }
                else if(email.ToLower().Contains("education.gov.uk"))
                {
                    // TOOO - temp work around for dfe accounts to get name
                    var address = email.Split('@').First();
                    givenname = address.Split('.').First();
                    surname = address.Split('.').Last();
                }
                
                
                return new UserDetailsViewModel()
                {
                    Id = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value),
                    Email = email,
                    Name = givenname + " " + surname,
                    Permissions = new [] { "team lead" },
                };
            }

            Response.StatusCode = 403;
            return null;
        }
    }
}