﻿#nullable enable
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

        Task<Unit> CreateAll(List<Guid> userIds,
            Guid releaseId,
            ReleaseRole role,
            Guid createdById);

        Task Remove(UserReleaseRole userReleaseRole, Guid deletedById);

        Task RemoveAll(List<UserReleaseRole> userReleaseRoles, Guid deletedById);

        Task RemoveAllUserReleaseRolesForPublication(
            Guid userId, Publication publication,
            ReleaseRole role, Guid deletedById);

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
