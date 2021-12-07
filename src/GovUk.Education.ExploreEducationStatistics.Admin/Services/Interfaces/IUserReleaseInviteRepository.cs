#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseInviteRepository
    {
        Task MarkInviteEmailAsSent(UserReleaseInvite invite);

        Task<bool> UserHasInvite(Guid releaseId, string email, ReleaseRole role);
    }
}
