using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserService : IUserService
    {
        public string GetLoggedInUserEmail(HttpContext http)
        {
            var email = http.User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            
            if (string.IsNullOrWhiteSpace(email))
            {
                email = http.User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
            }
            

            return email;
        }
    }
}