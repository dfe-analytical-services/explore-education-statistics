#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using LinqToDB.Async;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration;

/// <summary>
/// TODO EES-XXXX Remove after the User Resource Roles migration is complete.
/// </summary>
public class UserResourceRolesMigrationService(
    ContentDbContext contentDbContext,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserService userService
) : IUserResourceRolesMigrationService
{
    public async Task<Either<ActionResult, ThingDto>> MigrateUserResourceRoles(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    ) =>
        await userService
            .CheckIsBauUser()
            .OnSuccess(async () =>
            {
                var allDistinctUserResourceRolesGroupedByUserAndPublication =
                    await GetAllDistinctUserResourceRolesGroupedByUserAndPublication();

                var newPermissionsSystemChanges = allDistinctUserResourceRolesGroupedByUserAndPublication
                    .Select(DetermineNewPermissionsSystemChanges)
                    // If the User/Publication combination has no changes to be made, then we don't need to consider it further
                    .Where(changes =>
                        changes.NewSystemPublicationRoleToRemove.HasValue
                        || changes.NewSystemPublicationRoleToCreate.HasValue
                    )
                    .ToList();

                var newPermissionsSystemRoleKeysToRemove = newPermissionsSystemChanges
                    .Where(changes => changes.NewSystemPublicationRoleToRemove.HasValue)
                    .Select(changes =>
                        (
                            changes.UserId,
                            changes.PublicationId,
                            RoleToRemove: changes.NewSystemPublicationRoleToRemove!.Value
                        )
                    )
                    .ToHashSet();

                var newPermissionsSystemRoleKeysToCreate = newPermissionsSystemChanges
                    .Where(changes => changes.NewSystemPublicationRoleToCreate.HasValue)
                    .Select(changes =>
                        (
                            changes.UserId,
                            changes.PublicationId,
                            RoleToCreate: changes.NewSystemPublicationRoleToCreate!.Value
                        )
                    )
                    .ToHashSet();

                var newPermissionsSystemRolesToRemove = await userPublicationRoleRepository
                    .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
                    .Where(upr =>
                        newPermissionsSystemRoleKeysToRemove.Any(k =>
                            k.UserId == upr.UserId && k.PublicationId == upr.PublicationId && k.RoleToRemove == upr.Role
                        )
                    )
                    .ToListAsync();

                var newPermissionsSystemRolesToCreate = newPermissionsSystemRoleKeysToCreate
                    .Select(k => new UserPublicationRole
                    {
                        UserId = k.UserId,
                        PublicationId = k.PublicationId,
                        Role = k.RoleToCreate,
                        Created = DateTime.UtcNow, // Needs changing to whatever we think is appropriate
                        CreatedById = Guid.NewGuid(), // Needs changing to whatever we think is appropriate
                    })
                    .ToList();

                if (!dryRun)
                {
                    contentDbContext.UserPublicationRoles.RemoveRange(newPermissionsSystemRolesToRemove);
                    contentDbContext.UserPublicationRoles.AddRange(newPermissionsSystemRolesToCreate);
                    await contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return new ThingDto { DryRun = dryRun };
            });

    private async Task<List<GroupedUserResourceRoles>> GetAllDistinctUserResourceRolesGroupedByUserAndPublication()
    {
        var groupedUserPublicationRoles = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .Select(upr => new
            {
                upr.UserId,
                upr.PublicationId,
                upr.Role,
            })
            .GroupBy(a => new { a.UserId, a.PublicationId })
            // Check to see if .ToHashSet is done in-memory or SQL. If in SQL, change to g.Select(a => a.Role).Distinct().ToList() and then do
            // .ToHashSet() in-memory after the query.
            .Select(g => new GroupedUserPublicationRoles(
                g.Key.UserId,
                g.Key.PublicationId,
                g.Select(a => a.Role).ToHashSet()
            ))
            .ToListAsync();

        var groupedUserReleaseRoles = await userReleaseRoleRepository
            .Query(ResourceRoleFilter.All)
            .Select(urr => new
            {
                urr.UserId,
                urr.ReleaseVersion.Release.PublicationId,
                urr.Role,
            })
            // Becuase the same release roles can be across multiple release versions for the same publication, this can
            // return duplicate roles for the same publication. Hence, we do a .Distinct() to optimise the query.
            .Distinct()
            .GroupBy(a => new { a.UserId, a.PublicationId })
            // Check to see if .ToHashSet is done in-memory or SQL. If in SQL, change to g.Select(a => a.Role).Distinct().ToList() and then do
            // .ToHashSet() in-memory after the query.
            .Select(g => new GroupedUserReleaseRoles(
                g.Key.UserId,
                g.Key.PublicationId,
                g.Select(a => a.Role).ToHashSet()
            ))
            .ToListAsync();

        return
        [
            .. groupedUserPublicationRoles
                .Select(upr => new
                {
                    upr.UserId,
                    upr.PublicationId,
                    PublicationRoles = upr.Roles,
                    ReleaseRoles = new HashSet<ReleaseRole>(),
                })
                .Concat(
                    groupedUserReleaseRoles.Select(urr => new
                    {
                        urr.UserId,
                        urr.PublicationId,
                        PublicationRoles = new HashSet<PublicationRole>(),
                        ReleaseRoles = urr.Roles,
                    })
                )
                .GroupBy(a => new { a.UserId, a.PublicationId })
                .Select(g => new GroupedUserResourceRoles(
                    UserId: g.Key.UserId,
                    PublicationId: g.Key.PublicationId,
                    PublicationRoles: [.. g.SelectMany(x => x.PublicationRoles)],
                    ReleaseRoles: [.. g.SelectMany(x => x.ReleaseRoles)]
                )),
        ];
    }

    private NewPermissionsSystemChanges DetermineNewPermissionsSystemChanges(
        GroupedUserResourceRoles groupedUserResourceRoles
    )
    {
        PublicationRole? expectedNewSystemPublicationRole = DetermineExpectedNewSystemPublicationRole(
            groupedUserResourceRoles
        );

        // There should only ever be a MAXIMUM of ONE NEW system publication role for any User/Publication combination at any one time.
        // So if that is not the case, expect this to blow up so we can investigate further before doing any migration.
        var existingNewSystemPublicationRole = groupedUserResourceRoles
            .PublicationRoles.Where(PublicationRoleUtils.IsNewPermissionsSystemPublicationRole)
            .Cast<PublicationRole?>()
            .SingleOrDefault();

        NewPermissionsSystemChanges Changes(PublicationRole? remove, PublicationRole? create) =>
            new(
                UserId: groupedUserResourceRoles.UserId,
                PublicationId: groupedUserResourceRoles.PublicationId,
                NewSystemPublicationRoleToRemove: remove,
                NewSystemPublicationRoleToCreate: create
            );

        // If no OLD system roles map to any NEW system roles, and there isn't already a NEW system role for this User/Publication combination,
        // then there are no changes to be made for this User/Publication combination
        if (!expectedNewSystemPublicationRole.HasValue && !existingNewSystemPublicationRole.HasValue)
        {
            return Changes(remove: null, create: null);
        }

        // If no OLD system roles map to any NEW system roles, but there is already a NEW system role for this User/Publication combination,
        // then we need to remove the existing NEW system role for this User/Publication combination.
        // In theory, this scenario should never arise because the SYNCING code we added in EES-6148 should have ensured that there are no existing
        // NEW system roles without an equivalent OLD system role. But, we should still account for it just in case.
        if (!expectedNewSystemPublicationRole.HasValue)
        {
            return Changes(remove: existingNewSystemPublicationRole!.Value, create: null);
        }

        // If the OLD system roles map to a NEW system role, but there isn't already a NEW system role for this User/Publication combination,
        // then we need to create the NEW system role for this User/Publication combination.
        if (!existingNewSystemPublicationRole.HasValue)
        {
            return Changes(remove: null, create: expectedNewSystemPublicationRole.Value);
        }

        // If the OLD system roles map to the existing NEW system role, then there is nothing to change for this User/Publication combination.
        if (existingNewSystemPublicationRole == expectedNewSystemPublicationRole)
        {
            return Changes(remove: null, create: null);
        }

        // If the OLD system roles map to a different NEW system role from the existing NEW system role,
        // then we need to remove the existing NEW system role and create the expected NEW system role for this User/Publication combination.
        return Changes(remove: existingNewSystemPublicationRole.Value, create: expectedNewSystemPublicationRole.Value);
    }

    private static PublicationRole? DetermineExpectedNewSystemPublicationRole(
        GroupedUserResourceRoles groupedUserResourceRoles
    )
    {
        var oldSystemPublicationRolesNewSystemEquivalents = groupedUserResourceRoles
            .PublicationRoles.Where(r => !r.IsNewPermissionsSystemPublicationRole())
            .Select(PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole)
            .ToHashSet();

        var releaseRolesNewSystemEquivalents = groupedUserResourceRoles
            .ReleaseRoles.Select(releaseRole =>
                (
                    canConvertToNewPermissionsSystemPublicationRole: releaseRole.TryConvertToNewPermissionsSystemPublicationRole(
                        out var newSystemPublicationRole
                    ),
                    newSystemPublicationRole
                )
            )
            .Where(tuple => tuple.canConvertToNewPermissionsSystemPublicationRole)
            .Select(tuple => tuple.newSystemPublicationRole!.Value)
            .ToHashSet();

        var allEquivalentNewSystemPublicationRoles = oldSystemPublicationRolesNewSystemEquivalents
            .Union(releaseRolesNewSystemEquivalents)
            .ToHashSet();

        return allEquivalentNewSystemPublicationRoles.Contains(PublicationRole.Approver) ? PublicationRole.Approver
            : allEquivalentNewSystemPublicationRoles.Contains(PublicationRole.Drafter) ? PublicationRole.Drafter
            : null;
    }

    private record GroupedUserPublicationRoles(Guid UserId, Guid PublicationId, HashSet<PublicationRole> Roles);

    private record GroupedUserReleaseRoles(Guid UserId, Guid PublicationId, HashSet<ReleaseRole> Roles);

    private record GroupedUserResourceRoles(
        Guid UserId,
        Guid PublicationId,
        HashSet<PublicationRole> PublicationRoles,
        HashSet<ReleaseRole> ReleaseRoles
    );

    private record NewPermissionsSystemChanges(
        Guid UserId,
        Guid PublicationId,
        PublicationRole? NewSystemPublicationRoleToRemove,
        PublicationRole? NewSystemPublicationRoleToCreate
    );
}
