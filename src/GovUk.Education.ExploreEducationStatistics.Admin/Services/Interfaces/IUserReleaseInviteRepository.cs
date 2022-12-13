#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseInviteRepository
    {
        Task Create(Guid releaseId,
            string email,
            ReleaseRole releaseRole,
            bool emailSent,
            Guid createdById,
            DateTime? createdDate = null);

        Task CreateManyIfNotExists(List<Guid> releaseIds,
            string email,
            ReleaseRole releaseRole,
            bool emailSent,
            Guid createdById,
            DateTime? createdDate = null);

        Task Remove(Guid releaseId, string email, ReleaseRole role);

        Task<bool> UserHasInvite(Guid releaseId, string email, ReleaseRole role);

        Task<bool> UserHasInvites(List<Guid> releaseIds, string email, ReleaseRole role);

        Task RemoveByPublication(Publication publication, string email, ReleaseRole role);
    }
}
