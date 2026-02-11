#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleRepository(
    ContentDbContext contentDbContext,
    INewPermissionsSystemHelper newPermissionsSystemHelper,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    UserReleaseRoleQueryRepository userReleaseRoleQueryRepository,
    IUserRepository userRepository
) : IUserReleaseRoleRepository
{
    // This method will remain but be amended slightly in EES-6196, when we no longer have to cater for
    // the old roles.
    public async Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        createdDate ??= createdDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var publicationId = await GetPublicationId(releaseVersionId, cancellationToken);

        var allUserPublicationRolesForUserAndPublicationByRole = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .ToDictionaryAsync(upr => upr.Role, cancellationToken);

        var allPublicationRolesForUserAndPublication =
            allUserPublicationRolesForUserAndPublicationByRole.Keys.ToHashSet();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                existingPublicationRoles: allPublicationRolesForUserAndPublication,
                releaseRoleToCreate: role
            );

        if (newSystemPublicationRoleToRemove.HasValue)
        {
            var userPublicationRole = allUserPublicationRolesForUserAndPublicationByRole[
                newSystemPublicationRoleToRemove.Value
            ];

            await userPublicationRoleRepository.RemoveRole(userPublicationRole!, cancellationToken);
        }

        if (newSystemPublicationRoleToCreate.HasValue)
        {
            await userPublicationRoleRepository.CreateRole(
                userId: userId,
                publicationId: publicationId,
                role: newSystemPublicationRoleToCreate.Value,
                createdById: createdById,
                createdDate: createdDate!.Value,
                cancellationToken: cancellationToken
            );
        }

        var newUserReleaseRole = new UserReleaseRole
        {
            UserId = userId,
            ReleaseVersionId = releaseVersionId,
            Role = role,
            Created = createdDate!.Value,
            CreatedById = createdById,
        };

        contentDbContext.UserReleaseRoles.Add(newUserReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserReleaseRole;
    }

    public async Task<List<UserReleaseRole>> CreateManyIfNotExists(
        // I have purposefully not created a DTO for this method, to match the equivalent method in IUserPublicationRoleRepository.
        // Reason being, eventually the only release role which will exist is the pre-release role, which does not map to any NEW permissions system publication role.
        // At that point, this method will no longer need to call the Create method (which contains the logic to determine which OLD/NEW roles to create),
        // and will simply be adding UserReleaseRole entities to the DbContext directly.
        // I suspect this will be the case in EES-6196.
        // By leaving this as accepting a list of UserReleaseRole entities for now, we can avoid having to create yet another DTO which will only be used temporarily,
        // and avoid having to refactor the code/tests accordingly.
        IReadOnlyList<UserReleaseRole> userReleaseRolesToCreate,
        CancellationToken cancellationToken = default
    )
    {
        return await userReleaseRolesToCreate
            .ToAsyncEnumerable()
            .WhereAwait(async urr =>
                !await UserHasRoleOnReleaseVersion(
                    userId: urr.UserId,
                    releaseVersionId: urr.ReleaseVersionId,
                    role: urr.Role,
                    resourceRoleFilter: ResourceRoleFilter.All,
                    cancellationToken: cancellationToken
                )
            )
            .SelectAwait(async urr =>
                await Create(
                    userId: urr.UserId,
                    releaseVersionId: urr.ReleaseVersionId,
                    role: urr.Role,
                    // Need to check if all database values are non-null now. Maybe I can migrate this field to be non-nullable?
                    createdById: urr.CreatedById!.Value,
                    createdDate: urr.Created,
                    cancellationToken: cancellationToken
                )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<UserReleaseRole?> GetById(Guid userReleaseRoleId, CancellationToken cancellationToken = default)
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(urr => urr.Id == userReleaseRoleId, cancellationToken);
    }

    public async Task<UserReleaseRole?> GetByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly) =>
        userReleaseRoleQueryRepository.Query(resourceRoleFilter);

    // This method will remain but be amended slightly in EES-6196, when we no longer have to cater
    // for the old roles.
    public async Task<bool> RemoveById(Guid userReleaseRoleId, CancellationToken cancellationToken = default)
    {
        var userReleaseRole = await GetById(userReleaseRoleId, cancellationToken);

        if (userReleaseRole is null)
        {
            return false;
        }

        await Remove(userReleaseRole, cancellationToken);

        return true;
    }

    // This method will remain but be amended slightly in EES-6196, when we no longer have to cater
    // for the old roles.
    public async Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    )
    {
        var userReleaseRole = await GetByCompositeKey(
            userId: userId,
            releaseVersionId: releaseVersionId,
            role: role,
            cancellationToken: cancellationToken
        );

        if (userReleaseRole is null)
        {
            return false;
        }

        await Remove(userReleaseRole, cancellationToken);

        return true;
    }

    // This will be reverted back to removing the role entities from the DbContext directly in EES-6196, when
    // we no longer have to cater for the NEW publication roles within this repository (when only pre-release roles will exist).
    public async Task RemoveMany(HashSet<Guid> userReleaseRoleIds, CancellationToken cancellationToken = default)
    {
        if (!userReleaseRoleIds.Any())
        {
            return;
        }

        foreach (var userReleaseRoleId in userReleaseRoleIds)
        {
            await RemoveById(userReleaseRoleId, cancellationToken);
        }
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userReleaseRoles = await Query(ResourceRoleFilter.All)
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        contentDbContext.UserReleaseRoles.RemoveRange(userReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserHasRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ReleaseRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<ReleaseRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<ReleaseRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task MarkEmailAsSent(
        Guid userReleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userReleaseRole = await GetById(userReleaseRoleId: userReleaseRoleId, cancellationToken: cancellationToken);

        if (userReleaseRole is null)
        {
            throw new InvalidOperationException($"No User Release Role found with ID {userReleaseRoleId}.");
        }

        userReleaseRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Remove(UserReleaseRole userReleaseRole, CancellationToken cancellationToken = default)
    {
        var publicationId = await GetPublicationId(userReleaseRole.ReleaseVersionId, cancellationToken);

        var allReleaseRolesForUserAndPublication = (
            await Query(ResourceRoleFilter.All)
                .WhereForUser(userReleaseRole.UserId)
                .WhereForPublication(publicationId)
                .Select(urr => urr.Role)
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        var allUserPublicationRolesForUserAndPublicationByRole = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .WhereForUser(userReleaseRole.UserId)
            .WhereForPublication(publicationId)
            .ToDictionaryAsync(upr => upr.Role, cancellationToken);

        var allPublicationRolesForUserAndPublication =
            allUserPublicationRolesForUserAndPublicationByRole.Keys.ToHashSet();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                existingPublicationRoles: allPublicationRolesForUserAndPublication,
                existingReleaseRoles: allReleaseRolesForUserAndPublication,
                releaseRoleToRemove: userReleaseRole.Role
            );

        contentDbContext.UserReleaseRoles.Remove(userReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        if (newSystemPublicationRoleToRemove.HasValue)
        {
            var newSystemUserPublicationRoleToRemove = allUserPublicationRolesForUserAndPublicationByRole[
                newSystemPublicationRoleToRemove.Value
            ];

            await userPublicationRoleRepository.RemoveRole(newSystemUserPublicationRoleToRemove, cancellationToken);
        }

        if (newSystemPublicationRoleToCreate.HasValue)
        {
            var deletedUserPlaceholder = await userRepository.FindDeletedUserPlaceholder(cancellationToken);

            await userPublicationRoleRepository.CreateRole(
                userId: userReleaseRole.UserId,
                publicationId: publicationId,
                role: newSystemPublicationRoleToCreate.Value,
                createdById: deletedUserPlaceholder.Id,
                createdDate: DateTime.UtcNow,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task<Guid> GetPublicationId(Guid releaseVersionId, CancellationToken cancellationToken) =>
        await contentDbContext
            .ReleaseVersions.Where(r => r.Id == releaseVersionId)
            .Select(r => r.Release.PublicationId)
            .SingleAsync(cancellationToken);
}
