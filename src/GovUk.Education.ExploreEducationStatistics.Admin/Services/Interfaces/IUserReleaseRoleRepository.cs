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
            ReleaseRole role,
            Guid createdById);

        Task<UserReleaseRole> CreateIfNotExists(Guid userId,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task CreateManyIfNotExists(List<Guid> userIds,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task CreateManyIfNotExists(Guid userId,
            List<Guid> releaseIds,
            ReleaseRole role,
            Guid createdById);

        Task Remove(UserReleaseRole userReleaseRole, Guid deletedById);

        Task RemoveMany(List<UserReleaseRole> userReleaseRoles, Guid deletedById);

        Task RemoveAllForPublication(
            Guid userId, Publication publication,
            ReleaseRole role, Guid deletedById);

        Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId);
        
        Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId,
            Guid releaseId);

        Task<bool> IsUserPrereleaseViewerOnLatestPreReleaseRelease(Guid userId, Guid publicationId);

        Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseId, ReleaseRole[]? rolesToInclude);

        Task<bool> HasUserReleaseRole(Guid userId,
            Guid releaseId,
            ReleaseRole role);

        Task<bool> HasUserReleaseRole(string email,
            Guid releaseId,
            ReleaseRole role);
    }
}
