using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security
{
    public interface IUserService
    {
        Guid GetUserId();
        
        Task<bool> MatchesPolicy(Enum policy);

        Task<bool> MatchesPolicy(object resource, Enum policy);
    }
}