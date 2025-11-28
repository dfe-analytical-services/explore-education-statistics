#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(ContentDbContext contentDbContext) : IUserPublicationRoleRepository
{
    public async Task<UserPublicationRole> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var newUserPublicationRole = new UserPublicationRole
        {
            UserId = userId,
            PublicationId = publicationId,
            Role = role,
            Created = createdDate?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedById = createdById,
        };

        contentDbContext.UserPublicationRoles.Add(newUserPublicationRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPublicationRole;
    }

    public async Task CreateManyIfNotExists(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    )
    {
        var newUserPublicationRoles = await userPublicationRoles
            .ToAsyncEnumerable()
            .WhereAwait(async userPublicationRole =>
                !await UserHasRoleOnPublication(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role
                )
            )
            .ToListAsync(cancellationToken);

        contentDbContext.UserPublicationRoles.AddRange(newUserPublicationRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<UserPublicationRole> Query(
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active
    ) =>
        resourceRoleStatusFilter switch
        {
            ResourceRoleStatusFilter.Active => contentDbContext.UserPublicationRolesForActiveUsers,
            ResourceRoleStatusFilter.Pending => contentDbContext.UserPublicationRolesForPendingInvites,
            ResourceRoleStatusFilter.All => contentDbContext.UserPublicationRolesForActiveOrPending,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resourceRoleStatusFilter),
                resourceRoleStatusFilter,
                null
            ),
        };

    public async Task<UserPublicationRole?> GetUserPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query()
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default)
    {
        contentDbContext.UserPublicationRoles.Remove(userPublicationRole);

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userPublicationRoles.Any())
        {
            return;
        }

        contentDbContext.UserPublicationRoles.RemoveRange(userPublicationRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPublicationRoles = await contentDbContext
            .UserPublicationRoles.Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await RemoveMany(userPublicationRoles, cancellationToken);
    }

    public async Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleStatusFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleStatusFilter resourceRoleStatusFilter = ResourceRoleStatusFilter.Active,
        CancellationToken cancellationToken = default,
        params PublicationRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<PublicationRole>();

        return await Query(resourceRoleStatusFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
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
            role: role,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            throw new InvalidOperationException(
                $"No User Publication Role found for {nameof(userId)} '{userId}', {nameof(publicationId)} '{publicationId}', {nameof(role)} '{role}'"
            );
        }

        userPublicationRole.EmailSent = emailSent?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
