#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseRoleRepository
    {
        Task<UserReleaseRole> Create(Guid userId,
            Guid releaseId,
            ReleaseRole role);

        Task<List<ReleaseRole>> GetAllRolesByUser(Guid userId,
            Guid releaseId);

        Task<bool> IsUserApproverOnLatestRelease(Guid userId, Guid publicationId);

        Task<bool> IsUserEditorOrApproverOnLatestRelease(Guid userId, Guid publicationId);

        Task<bool> UserHasRoleOnRelease(Guid userId,
            Guid releaseId,
            ReleaseRole role);

        Task<bool> UserHasAnyOfRolesOnLatestRelease(Guid userId,
            Guid publicationId,
            IEnumerable<ReleaseRole> roles);
    }
}
