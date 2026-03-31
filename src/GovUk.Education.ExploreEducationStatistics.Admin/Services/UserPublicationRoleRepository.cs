#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(
    ContentDbContext contentDbContext,
    IPublicationRoleChangesHelper publicationRoleChangesHelper
) : IUserPublicationRoleRepository
{
    public async Task<UserPublicationRole?> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{role}'. Expected a NEW permissions system role."
            );
        }

        createdDate ??= createdDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var existingUserPublicationRole = await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .SingleOrDefaultAsync(cancellationToken);

        var (publicationRoleToRemove, publicationRoleToCreate) = publicationRoleChangesHelper.DetermineChanges(
            existingPublicationRoleForPublication: existingUserPublicationRole?.Role,
            publicationRoleToCreate: role
        );

        if (publicationRoleToRemove.HasValue)
        {
            if (existingUserPublicationRole == null)
            {
                throw new InvalidOperationException(
                    $"A publication role ('{publicationRoleToRemove.Value}') was expected to be removed for user '{userId}' on publication '{publicationId}', but no existing user publication role was found."
                );
            }

            if (publicationRoleToRemove.Value != existingUserPublicationRole.Role)
            {
                throw new InvalidOperationException(
                    $"The publication role to remove ('{publicationRoleToRemove.Value}') does not match the existing user publication role ('{existingUserPublicationRole.Role}')."
                );
            }

            await RemoveRole(existingUserPublicationRole, cancellationToken);
        }

        if (!publicationRoleToCreate.HasValue)
        {
            return null;
        }

        var newUserPublicationRole = new UserPublicationRole
        {
            UserId = userId,
            PublicationId = publicationId,
            Role = publicationRoleToCreate.Value,
            Created = createdDate!.Value,
            CreatedById = createdById,
        };

        contentDbContext.UserPublicationRoles.Add(newUserPublicationRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPublicationRole;
    }

    public async Task<List<UserPublicationRole>> CreateManyIfNotExists(
        HashSet<UserPublicationRoleCreateDto> userPublicationRolesToCreate,
        CancellationToken cancellationToken = default
    )
    {
        if (userPublicationRolesToCreate.Any(dto => !dto.Role.IsNewPermissionsSystemPublicationRole()))
        {
            throw new ArgumentException(
                $"Unexpected publication role found in the list of roles to create. All roles should be NEW permissions system roles."
            );
        }

        return await userPublicationRolesToCreate
            .ToAsyncEnumerable()
            .Where(
                async (upr, ct) =>
                    !await UserHasRoleOnPublication(
                        userId: upr.UserId,
                        publicationId: upr.PublicationId,
                        role: upr.Role,
                        resourceRoleFilter: ResourceRoleFilter.All,
                        cancellationToken: ct
                    )
            )
            .Select(
                async (upr, ct) =>
                    await Create(
                        userId: upr.UserId,
                        publicationId: upr.PublicationId,
                        role: upr.Role,
                        createdById: upr.CreatedById,
                        createdDate: upr.CreatedDate,
                        cancellationToken: ct
                    )
            )
            .WhereNotNull()
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPublicationRole?> GetById(
        Guid userPublicationRoleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(upr => upr.Id == userPublicationRoleId, cancellationToken);
    }

    public async Task<UserPublicationRole?> GetByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserPublicationRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly)
    {
        var userPublicationRoles = contentDbContext.UserPublicationRoles.AsQueryable();

        return resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => userPublicationRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => userPublicationRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => userPublicationRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => userPublicationRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
    }

    public async Task<bool> RemoveById(Guid userPublicationRoleId, CancellationToken cancellationToken = default)
    {
        var userPublicationRole = await GetById(
            userPublicationRoleId: userPublicationRoleId,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            return false;
        }

        await RemoveRole(userPublicationRole, cancellationToken);

        return true;
    }

    public async Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    )
    {
        var userPublicationRole = await GetByCompositeKey(
            userId: userId,
            publicationId: publicationId,
            role: role,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            return false;
        }

        await RemoveRole(userPublicationRole, cancellationToken);

        return true;
    }

    public async Task RemoveMany(
        HashSet<UserPublicationRole> userPublicationRoles,
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
        var userPublicationRoles = await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .ToListAsync(cancellationToken);

        contentDbContext.UserPublicationRoles.RemoveRange(userPublicationRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params PublicationRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<PublicationRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task MarkEmailAsSent(
        Guid userPublicationRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userPublicationRole = await GetById(
            userPublicationRoleId: userPublicationRoleId,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            throw new InvalidOperationException($"No User Publication Role found with ID {userPublicationRoleId}.");
        }

        userPublicationRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RemoveRole(UserPublicationRole userPublicationRole, CancellationToken cancellationToken)
    {
        if (!userPublicationRole.Role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{userPublicationRole.Role}'. Expected a NEW permissions system role."
            );
        }

        contentDbContext.UserPublicationRoles.Remove(userPublicationRole);

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public record UserPublicationRoleCreateDto(
        Guid UserId,
        Guid PublicationId,
        PublicationRole Role,
        Guid CreatedById,
        DateTime? CreatedDate = null
    );
}
