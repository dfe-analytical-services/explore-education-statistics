using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserService
    {
        string GetLoggedInUserEmail(HttpContext http);
    }
}