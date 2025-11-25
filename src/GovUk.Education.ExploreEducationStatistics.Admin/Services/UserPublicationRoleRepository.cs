#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(ContentDbContext contentDbContext)
    : UserResourceRoleRepositoryBase<UserPublicationRoleRepository, UserPublicationRole, Publication, PublicationRole>(
        contentDbContext
    ),
        IUserPublicationRoleRepository
{
    protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceId(Guid publicationId)
    {
        return ContentDbContext.UserPublicationRoles.Where(role => role.PublicationId == publicationId);
    }

    protected override IQueryable<UserPublicationRole> GetResourceRolesQueryByResourceIds(List<Guid> publicationIds)
    {
        return ContentDbContext.UserPublicationRoles.Where(role => publicationIds.Contains(role.PublicationId));
    }

    public async Task<List<PublicationRole>> GetDistinctRolesByUser(Guid userId)
    {
        return await GetDistinctResourceRolesByUser(userId);
    }

    public async Task<List<PublicationRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return await GetAllResourceRolesByUserAndResource(userId, publicationId);
    }

    public async Task<UserPublicationRole?> GetUserPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role
    )
    {
        return await GetResourceRole(userId, publicationId, role);
    }

    public async Task<bool> UserHasRoleOnPublication(Guid userId, Guid publicationId, PublicationRole role)
    {
        return await UserHasRoleOnResource(userId, publicationId, role);
    }

    public new async Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default)
    {
        await base.Remove(userPublicationRole, cancellationToken);
    }

    public new async Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    )
    {
        await base.RemoveMany(userPublicationRoles, cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPublicationRoles = await ContentDbContext
            .UserPublicationRoles.Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await base.RemoveMany(userPublicationRoles, cancellationToken);
    }

    // The optional 'emailSent' date parameter will be removed in EES-6511 when we stop using UserReleaseRoleInvites/UserPublicationRoleInvites
    // altogether. At that point, a role will always exist at the point of sending an email; which means we can always set the date at the point
    // of sending.
    public async Task MarkEmailAsSent(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        DateTimeOffset? emailSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userPublicationRole = await GetUserPublicationRole(
            userId: userId,
            publicationId: publicationId,
            role: role
        );

        if (userPublicationRole is null)
        {
            throw new InvalidOperationException(
                $"No User Publication Role found for {nameof(userId)} '{userId}', {nameof(publicationId)} '{publicationId}', {nameof(role)} '{role}'"
            );
        }

        userPublicationRole.EmailSent = emailSent?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        await ContentDbContext.SaveChangesAsync(cancellationToken);
    }
}
