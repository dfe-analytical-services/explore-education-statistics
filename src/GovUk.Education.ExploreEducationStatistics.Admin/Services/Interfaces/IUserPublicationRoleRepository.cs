#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserPublicationRoleAndInviteManager
{
    Task<UserPublicationRole> Create(Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById);

    Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId);

    Task<List<PublicationRole>> GetAllRolesByUserAndPublication(
        Guid userId,
        Guid publicationId);

    Task<UserPublicationRole?> GetUserPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role);

    Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role);

    Task RemoveRoleAndInvite(
        UserPublicationRole userPublicationRole,
        CancellationToken cancellationToken = default);

    Task RemoveRolesAndInvites(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default);

    Task RemoveAllRolesAndInvitesForUser(
        Guid userId,
        CancellationToken cancellationToken = default);
}
