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
    INewPermissionsSystemHelper newPermissionsSystemHelper,
    UserReleaseRoleQueryRepository userReleaseRoleQueryRepository,
    IUserRepository userRepository
) : IUserPublicationRoleRepository
{
    // This method will remain but be amended slightly in EES-6196, when we no longer have to cater for
    // the old roles.
    public async Task<UserPublicationRole?> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        if (role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{role}'. Expected an OLD permissions system role."
            );
        }

        createdDate ??= createdDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var allUserPublicationRolesForUserAndPublicationByRole = await Query(
                ResourceRoleFilter.All,
                includeNewPermissionsSystemRoles: true
            )
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .ToDictionaryAsync(upr => upr.Role, cancellationToken);

        var allPublicationRolesForUserAndPublication =
            allUserPublicationRolesForUserAndPublicationByRole.Keys.ToHashSet();

        var allReleaseRolesForUserAndPublication = (
            await userReleaseRoleQueryRepository
                .Query(ResourceRoleFilter.All)
                .WhereForUser(userId)
                .WhereForPublication(publicationId)
                .Select(urr => urr.Role)
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                existingPublicationRoles: allPublicationRolesForUserAndPublication,
                existingReleaseRoles: allReleaseRolesForUserAndPublication,
                publicationRoleToCreate: role
            );

        if (newSystemPublicationRoleToRemove.HasValue)
        {
            var userPublicationRole = allUserPublicationRolesForUserAndPublicationByRole[
                newSystemPublicationRoleToRemove.Value
            ];

            await RemoveRole(userPublicationRole!, cancellationToken);
        }

        UserPublicationRole? createdNewPermissionsSystemPublicationRole = null;

        if (newSystemPublicationRoleToCreate.HasValue)
        {
            createdNewPermissionsSystemPublicationRole = await CreateRole(
                userId: userId,
                publicationId: publicationId,
                role: newSystemPublicationRoleToCreate.Value,
                createdById: createdById,
                createdDate: createdDate!.Value,
                cancellationToken: cancellationToken
            );
        }

        return role.IsNewPermissionsSystemPublicationRole()
            ? createdNewPermissionsSystemPublicationRole
            : await CreateRole(
                userId: userId,
                publicationId: publicationId,
                role: role,
                createdById: createdById,
                createdDate: createdDate!.Value,
                cancellationToken: cancellationToken
            );
    }

    public async Task<List<UserPublicationRole>> CreateManyIfNotExists(
        HashSet<UserPublicationRoleCreateDto> userPublicationRolesToCreate,
        CancellationToken cancellationToken = default
    )
    {
        if (userPublicationRolesToCreate.Any(dto => dto.Role.IsNewPermissionsSystemPublicationRole()))
        {
            throw new ArgumentException(
                $"Unexpected publication role found in the list of roles to create. All roles should be OLD permissions system roles."
            );
        }

        return await userPublicationRolesToCreate
            .ToAsyncEnumerable()
            .Where(
                async (upr, cancellationToken) =>
                    !await UserHasRoleOnPublication(
                        userId: upr.UserId,
                        publicationId: upr.PublicationId,
                        role: upr.Role,
                        resourceRoleFilter: ResourceRoleFilter.All,
                        cancellationToken: cancellationToken
                    )
            )
            .Select(
                async (upr, cancellationToken) =>
                    await Create(
                        userId: upr.UserId,
                        publicationId: upr.PublicationId,
                        role: upr.Role,
                        createdById: upr.CreatedById,
                        createdDate: upr.CreatedDate,
                        cancellationToken: cancellationToken
                    )
            )
            .WhereNotNull()
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPublicationRole?> GetById(
        Guid userPublicationRoleId,
        bool includeNewPermissionsSystemRoles = false,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles)
            .SingleOrDefaultAsync(upr => upr.Id == userPublicationRoleId, cancellationToken);
    }

    public async Task<UserPublicationRole?> GetByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        bool includeNewPermissionsSystemRoles = false,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserPublicationRole> Query(
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        bool includeNewPermissionsSystemRoles = false
    )
    {
        var userPublicationRoles = contentDbContext.UserPublicationRoles.AsQueryable();

        if (includeNewPermissionsSystemRoles)
        {
            userPublicationRoles = userPublicationRoles.IgnoreQueryFilters();
        }

        return resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => userPublicationRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => userPublicationRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => userPublicationRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => userPublicationRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
    }

    // This method will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
    public async Task<bool> RemoveById(Guid userPublicationRoleId, CancellationToken cancellationToken = default)
    {
        var userPublicationRole = await GetById(
            userPublicationRoleId: userPublicationRoleId,
            includeNewPermissionsSystemRoles: true,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            return false;
        }

        await Remove(userPublicationRole, cancellationToken);

        return true;
    }

    // This method will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
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
            includeNewPermissionsSystemRoles: true,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            return false;
        }

        await Remove(userPublicationRole, cancellationToken);

        return true;
    }

    private async Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default)
    {
        if (userPublicationRole.Role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{userPublicationRole.Role}'. Expected an OLD permissions system role."
            );
        }

        var allUserPublicationRolesForUserAndPublicationByRole = await Query(
                ResourceRoleFilter.All,
                includeNewPermissionsSystemRoles: true
            )
            .WhereForUser(userPublicationRole.UserId)
            .WhereForPublication(userPublicationRole.PublicationId)
            .ToDictionaryAsync(upr => upr.Role, cancellationToken);

        var allPublicationRolesForUserAndPublication =
            allUserPublicationRolesForUserAndPublicationByRole.Keys.ToHashSet();

        var allReleaseRolesForUserAndPublication = (
            await userReleaseRoleQueryRepository
                .Query(ResourceRoleFilter.All)
                .WhereForUser(userPublicationRole.UserId)
                .WhereForPublication(userPublicationRole.PublicationId)
                .Select(urr => urr.Role)
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                existingPublicationRoles: allPublicationRolesForUserAndPublication,
                existingReleaseRoles: allReleaseRolesForUserAndPublication,
                oldPublicationRoleToRemove: userPublicationRole.Role
            );

        await RemoveRole(userPublicationRole, cancellationToken);

        if (newSystemPublicationRoleToRemove.HasValue)
        {
            var newSystemUserPublicationRoleToRemove = allUserPublicationRolesForUserAndPublicationByRole[
                newSystemPublicationRoleToRemove.Value
            ];

            await RemoveRole(newSystemUserPublicationRoleToRemove, cancellationToken);
        }

        if (newSystemPublicationRoleToCreate.HasValue)
        {
            var deletedUserPlaceholder = await userRepository.FindDeletedUserPlaceholder(cancellationToken);

            await CreateRole(
                userId: userPublicationRole.UserId,
                publicationId: userPublicationRole.PublicationId,
                role: newSystemPublicationRoleToCreate.Value,
                createdById: deletedUserPlaceholder.Id,
                createdDate: DateTime.UtcNow,
                cancellationToken: cancellationToken
            );
        }
    }

    // This will be reverted back to removing the role entities from the DbContext directly in EES-6196, when
    // we no longer have to cater for the old roles.
    public async Task RemoveMany(HashSet<Guid> userPublicationRoleIds, CancellationToken cancellationToken = default)
    {
        if (!userPublicationRoleIds.Any())
        {
            return;
        }

        foreach (var userPublicationRoleId in userPublicationRoleIds)
        {
            await RemoveById(userPublicationRoleId, cancellationToken);
        }
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPublicationRoles = await Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .Where(upr => upr.UserId == userId)
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
        return await Query(resourceRoleFilter, includeNewPermissionsSystemRoles: true)
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

    public async Task<UserPublicationRole> CreateRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime createdDate,
        CancellationToken cancellationToken
    )
    {
        var newUserPublicationRole = new UserPublicationRole
        {
            UserId = userId,
            PublicationId = publicationId,
            Role = role,
            Created = createdDate,
            CreatedById = createdById,
        };

        contentDbContext.UserPublicationRoles.Add(newUserPublicationRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPublicationRole;
    }

    public async Task RemoveRole(UserPublicationRole userPublicationRole, CancellationToken cancellationToken)
    {
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
