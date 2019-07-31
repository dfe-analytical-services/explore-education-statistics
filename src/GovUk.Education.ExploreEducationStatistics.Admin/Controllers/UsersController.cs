using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class UsersController : Controller
    {
        // GET
        public ActionResult<UserDetailsViewModel> MyDetails()
        {
            // TODO - we need to something cleverer here - we need to 
            // validate that their AD cookie is still valid, not simply
            // to check for its existence.  However, this is just in here
            // temporarily as a stopgap
            if (User.Identity.IsAuthenticated)
            {
                return new UserDetailsViewModel()
                {
                    Id = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value),
                    Email = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value,
                    Name = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value + " " + User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value,
                    Permissions = new string[] { "team lead" },
                };
            }

            Response.StatusCode = 403;
            return null;
        }
    }
}