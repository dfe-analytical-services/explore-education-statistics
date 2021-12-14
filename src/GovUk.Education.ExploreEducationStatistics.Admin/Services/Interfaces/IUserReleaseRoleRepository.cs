#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseRoleRepository
    {
        Task<UserReleaseRole> Create(Guid userId,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task<UserReleaseRole> CreateIfNotExists(Guid userId,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task<Unit> CreateManyIfNotExists(List<Guid> userIds,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task<Unit> CreateManyIfNotExists(Guid userId,
            List<Guid> releaseIds,
            ReleaseRole role,
            Guid createdById);

        Task Remove(UserReleaseRole userReleaseRole, Guid deletedById);

        Task RemoveMany(List<UserReleaseRole> userReleaseRoles, Guid deletedById);

        Task RemoveAllForPublication(
            Guid userId, Publication publication,
            ReleaseRole role, Guid deletedById);

        Task<List<ReleaseRole>> GetAllRolesByUser(Guid userId,
            Guid releaseId);

        Task<bool> IsUserApproverOnLatestRelease(Guid userId, Guid publicationId);

        Task<bool> IsUserEditorOrApproverOnLatestRelease(Guid userId, Guid publicationId);

        Task<UserReleaseRole?> GetUserReleaseRole(Guid userId,
            Guid releaseId,
            ReleaseRole role);

        Task<UserReleaseRole?> GetUserReleaseRole(string email,
            Guid releaseId,
            ReleaseRole role);

        Task<bool> UserHasAnyOfRolesOnLatestRelease(Guid userId,
            Guid publicationId,
            IEnumerable<ReleaseRole> roles);
    }
}
