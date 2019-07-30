using System;
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
            if (Request.Cookies.ContainsKey(".AspNetCore.AzureADCookie"))
            {
                return new UserDetailsViewModel()
                {
                    Id = Guid.Parse("4add7621-4aef-4abc-b2e6-0938b37fe5b9"),
                    Email = "john.smith@example.com",
                    Name = "John Smith",
                    Permissions = new string[] { "team lead" },
                };
            }

            Response.StatusCode = 403;
            return null;
        }
    }
}