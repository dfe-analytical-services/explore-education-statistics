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
    UserReleaseRoleQueryRepository userReleaseRoleQueryRepository
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

        var publicationId = await contentDbContext
            .ReleaseVersions.Where(r => r.Id == releaseVersionId)
            .Select(r => r.Release.PublicationId)
            .SingleAsync(cancellationToken);

        var existingUserPublicationRoles = (
            await userPublicationRoleRepository
                .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
                .WhereForUser(userId)
                .WhereForPublication(publicationId)
                .Select(upr => upr.Role)
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChanges(
                existingPublicationRoles: existingUserPublicationRoles,
                releaseRoleToCreate: role
            );

        if (newSystemPublicationRoleToRemove.HasValue)
        {
            var userPublicationRole = await userPublicationRoleRepository.GetByCompositeKey(
                userId: userId,
                publicationId: publicationId,
                role: newSystemPublicationRoleToRemove.Value,
                cancellationToken: cancellationToken,
                includeNewPermissionsSystemRoles: true
            );

            await userPublicationRoleRepository.Remove(userPublicationRole!, cancellationToken);
        }

        if (newSystemPublicationRoleToCreate.HasValue)
        {
            await userPublicationRoleRepository.Create(
                userId: userId,
                publicationId: publicationId,
                role: newSystemPublicationRoleToCreate.Value,
                createdById: createdById,
                createdDate: createdDate,
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
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        var newUserReleaseRoles = await userReleaseRoles
            .ToAsyncEnumerable()
            .WhereAwait(async userReleaseRole =>
                !await UserHasRoleOnReleaseVersion(
                    userId: userReleaseRole.UserId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    role: userReleaseRole.Role,
                    resourceRoleFilter: ResourceRoleFilter.All,
                    cancellationToken: cancellationToken
                )
            )
            .ToListAsync(cancellationToken);

        contentDbContext.UserReleaseRoles.AddRange(newUserReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserReleaseRoles;
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
    public async Task Remove(UserReleaseRole userReleaseRole, CancellationToken cancellationToken = default)
    {
        var allReleaseRolesForUserAndPublication = (
            await Query(ResourceRoleFilter.All)
                .WhereForUser(userReleaseRole.UserId)
                .WhereForPublication(userReleaseRole.ReleaseVersion.Release.PublicationId)
                .Select(urr => urr.Role)
                .ToListAsync(cancellationToken)
        ).ToHashSet();

        var allUserPublicationRolesForUserAndPublicationByRole = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .WhereForUser(userReleaseRole.UserId)
            .WhereForPublication(userReleaseRole.ReleaseVersion.Release.PublicationId)
            .ToDictionaryAsync(upr => upr.Role, cancellationToken);

        var allPublicationRolesForUserAndPublication =
            allUserPublicationRolesForUserAndPublicationByRole.Keys.ToHashSet();

        var newSystemPublicationRoleToRemove = newPermissionsSystemHelper.DetermineNewPermissionsSystemRoleToRemove(
            existingPublicationRoles: allPublicationRolesForUserAndPublication,
            existingReleaseRoles: allReleaseRolesForUserAndPublication,
            releaseRoleToRemove: userReleaseRole.Role
        );

        contentDbContext.UserReleaseRoles.Remove(userReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        if (!newSystemPublicationRoleToRemove.HasValue)
        {
            return;
        }

        var newSystemUserPublicationRoleToRemove = allUserPublicationRolesForUserAndPublicationByRole[
            newSystemPublicationRoleToRemove.Value
        ];

        await userPublicationRoleRepository.Remove(newSystemUserPublicationRoleToRemove, cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserReleaseRole> userReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userReleaseRoles.Any())
        {
            return;
        }

        contentDbContext.UserReleaseRoles.RemoveRange(userReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userReleaseRoles = await Query(ResourceRoleFilter.All)
            .Where(urr => urr.UserId == userId)
            .ToListAsync(cancellationToken);

        await RemoveMany(userReleaseRoles, cancellationToken);
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
}
