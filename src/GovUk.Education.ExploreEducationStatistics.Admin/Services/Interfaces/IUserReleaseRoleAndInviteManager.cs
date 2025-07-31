#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserReleaseRoleAndInviteManager
{
    Task<UserReleaseRole> Create(Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById);

    Task<UserReleaseRole> CreateIfNotExists(Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById);

    Task CreateManyIfNotExists(List<Guid> userIds,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById);

    Task CreateManyIfNotExists(Guid userId,
        List<Guid> releaseVersionIds,
        ReleaseRole role,
        Guid createdById);

    Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId);

    Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId,
        Guid releaseVersionId);

    Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId,
        Guid publicationId);

    Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseVersionId,
        ReleaseRole[]? rolesToInclude);

    Task<bool> HasUserReleaseRole(Guid userId,
        Guid releaseVersionId,
        ReleaseRole role);

    Task<bool> HasUserReleaseRole(string email,
        Guid releaseVersionId,
        ReleaseRole role);

    Task RemoveRoleAndInvite(
        UserReleaseRole userReleaseRole,
        CancellationToken cancellationToken = default);

    Task RemoveRolesAndInvites(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default);

    Task RemoveAllRolesAndInvitesForPublication(
        Guid publicationId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude);

    Task RemoveAllRolesAndInvitesForReleaseVersion(
        Guid releaseVersionId,
        Guid? userId = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude);

    Task RemoveAllRolesAndInvitesForUser(
        Guid userId,
        CancellationToken cancellationToken = default);
}
