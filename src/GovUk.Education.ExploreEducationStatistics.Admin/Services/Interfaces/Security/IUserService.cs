using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security
{
    public interface IUserService
    {
        Guid GetUserId();
        
        Task<bool> MatchesPolicy(SecurityPolicies policy);

        Task<bool> MatchesPolicy(object resource, SecurityPolicies policy);
    }
}