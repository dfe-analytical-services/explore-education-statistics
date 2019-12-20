using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserService
    {
        Guid GetUserId();
        
        Task<bool> MatchesPolicy(SecurityPolicies policy);


        Task<bool> MatchesPolicy(object resource, SecurityPolicies policy);
    }
}