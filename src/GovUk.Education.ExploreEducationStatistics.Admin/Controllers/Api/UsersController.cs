using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
//    [ApiExplorerSettings(IgnoreApi=true)]
    public class UsersController : ControllerBase
    {
        // GET
        [HttpGet("mydetails")]
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

                
                // TOOO - temp work around for dfe accounts to get name
                if (email.ToLower().Contains("education.gov.uk"))
                {
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