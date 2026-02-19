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
    public async Task<Either<ActionResult, UserResourceRolesMigrationReportDto>> MigrateUserResourceRoles(
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
                        changes.NewSystemPublicationRoleToRemove is not null
                        || changes.NewSystemPublicationRoleToCreate is not null
                    )
                    .ToList();

                var newPermissionsSystemRoleIdsToRemove = newPermissionsSystemChanges
                    .Where(changes => changes.NewSystemPublicationRoleToRemove is not null)
                    .Select(changes => changes.NewSystemPublicationRoleToRemove!.Id)
                    .ToHashSet();

                // We're expecting the number of removals to be pretty small. The SYNCING code we added in EES-6148
                // will not have been running for too long by the time this migration has ran. And the only time we expect a NEW
                // role to be removed here, is when the OLD and NEW systems get out of SYNC. That is only currenly possible in a couple of scenarios.
                // Namely: where a User has some OLD system roles that would map to a NEW system role of 'Approver', but the SYNCING code was
                // deployed after those OLD roles were added. Then, an OLD role of Publication 'Owner', or Release 'Contributor', was added
                // to the user for the same Publication and therefore the SYNCING code created a NEW system role of 'Drafter' for them.
                // In this case, they should really be an 'Approver' in the NEW system, but the SYNCING code wasn't made smart enough to detect this.
                // It was written to keep the systems in SYNC, rather than to check ALL existing roles for the User/Publication combination
                // on every role creation/removal. Once this migration has ran, however, the SYNCING code should work as expected and we should
                // not be able to get into this scenario again.
                var newPermissionsSystemRolesToRemove = await userPublicationRoleRepository
                    .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
                    .Where(upr => newPermissionsSystemRoleIdsToRemove.Contains(upr.Id))
                    .ToListAsync();

                var newPermissionsSystemRolesToCreate = newPermissionsSystemChanges
                    .Where(changes => changes.NewSystemPublicationRoleToCreate is not null)
                    .Select(changes => new UserPublicationRole
                    {
                        UserId = changes.UserId,
                        PublicationId = changes.PublicationId,
                        Role = changes.NewSystemPublicationRoleToCreate!.Role,
                        Created = changes.NewSystemPublicationRoleToCreate.CreatedDate,
                        CreatedById = changes.NewSystemPublicationRoleToCreate.CreatedById,
                        EmailSent = changes.NewSystemPublicationRoleToCreate.EmailSent,
                    })
                    .ToList();

                var numberOfDrafterRolesRemoved = newPermissionsSystemRolesToRemove.Count(upr =>
                    upr.Role == PublicationRole.Drafter
                );
                var numberOfDrafterRolesCreated = newPermissionsSystemRolesToCreate.Count(upr =>
                    upr.Role == PublicationRole.Drafter
                );
                var numberOfApproverRolesRemoved = newPermissionsSystemRolesToRemove.Count(upr =>
                    upr.Role == PublicationRole.Approver
                );
                var numberOfApproverRolesCreated = newPermissionsSystemRolesToCreate.Count(upr =>
                    upr.Role == PublicationRole.Approver
                );

                if (!dryRun)
                {
                    contentDbContext.UserPublicationRoles.RemoveRange(newPermissionsSystemRolesToRemove);
                    contentDbContext.UserPublicationRoles.AddRange(newPermissionsSystemRolesToCreate);
                    await contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return new UserResourceRolesMigrationReportDto
                {
                    DryRun = dryRun,
                    NumberOfDrafterRolesRemoved = numberOfDrafterRolesRemoved,
                    NumberOfDrafterRolesCreated = numberOfDrafterRolesCreated,
                    NumberOfApproverRolesRemoved = numberOfApproverRolesRemoved,
                    NumberOfApproverRolesCreated = numberOfApproverRolesCreated,
                };
            });

    private async Task<List<GroupedUserResourceRoles>> GetAllDistinctUserResourceRolesGroupedByUserAndPublication()
    {
        var groupedUserPublicationRoles = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.All, includeNewPermissionsSystemRoles: true)
            .GroupBy(upr => new { upr.UserId, upr.PublicationId })
            // Check to see if .ToHashSet is done in-memory or SQL. If in SQL, change to g.Select(a => a.Role).Distinct().ToList() and then do
            // .ToHashSet() in-memory after the query.
            .Select(g => new GroupedUserPublicationRoles(
                g.Key.UserId,
                g.Key.PublicationId,
                g.Select(upr => new ExistingPublicationRoleDetails(
                        upr.Id,
                        upr.Role,
                        upr.Created,
                        upr.CreatedById,
                        upr.EmailSent
                    ))
                    .ToHashSet()
            ))
            .ToListAsync();

        var groupedUserReleaseRoles = await userReleaseRoleRepository
            .Query(ResourceRoleFilter.All)
            .Select(urr => new
            {
                urr.Id,
                urr.UserId,
                urr.ReleaseVersion.Release.PublicationId,
                urr.Role,
                urr.Created,
                urr.CreatedById,
                urr.EmailSent,
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
                g.Select(a => new ExistingReleaseRoleDetails(a.Id, a.Role, a.Created, a.CreatedById, a.EmailSent))
                    .ToHashSet()
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
                    ReleaseRoles = new HashSet<ExistingReleaseRoleDetails>(),
                })
                .Concat(
                    groupedUserReleaseRoles.Select(urr => new
                    {
                        urr.UserId,
                        urr.PublicationId,
                        PublicationRoles = new HashSet<ExistingPublicationRoleDetails>(),
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
        NewPublicationRoleDetails? expectedNewSystemPublicationRoleDetails =
            DetermineExpectedNewSystemPublicationRoleDetails(groupedUserResourceRoles);

        // There should only ever be a MAXIMUM of ONE NEW system publication role for any User/Publication combination at any one time.
        // So if that is not the case, expect this to blow up so we can investigate further before doing any migration.
        var existingNewSystemPublicationRole = groupedUserResourceRoles
            .PublicationRoles.Where(epr => PublicationRoleUtils.IsNewPermissionsSystemPublicationRole(epr.Role))
            .SingleOrDefault();

        NewPermissionsSystemChanges Changes(
            ExistingPublicationRoleDetails? remove,
            NewPublicationRoleDetails? create
        ) =>
            new(
                UserId: groupedUserResourceRoles.UserId,
                PublicationId: groupedUserResourceRoles.PublicationId,
                NewSystemPublicationRoleToRemove: remove,
                NewSystemPublicationRoleToCreate: create
            );

        // If no OLD system roles map to any NEW system roles, and there isn't already a NEW system role for this User/Publication combination,
        // then there are no changes to be made for this User/Publication combination
        if (expectedNewSystemPublicationRoleDetails is null && existingNewSystemPublicationRole is null)
        {
            return Changes(remove: null, create: null);
        }

        // If no OLD system roles map to any NEW system roles, but there is already a NEW system role for this User/Publication combination,
        // then we need to remove the existing NEW system role for this User/Publication combination.
        // In theory, this scenario should never arise because the SYNCING code we added in EES-6148 should have ensured that there are no existing
        // NEW system roles without an equivalent OLD system role. But, we should still account for it just in case.
        if (expectedNewSystemPublicationRoleDetails is null)
        {
            return Changes(remove: existingNewSystemPublicationRole, create: null);
        }

        // If the OLD system roles map to a NEW system role, but there isn't already a NEW system role for this User/Publication combination,
        // then we need to create the NEW system role for this User/Publication combination.
        if (existingNewSystemPublicationRole is null)
        {
            return Changes(remove: null, create: expectedNewSystemPublicationRoleDetails);
        }

        // If the OLD system roles map to the existing NEW system role, then there is nothing to change for this User/Publication combination.
        if (existingNewSystemPublicationRole.Role == expectedNewSystemPublicationRoleDetails.Role)
        {
            return Changes(remove: null, create: null);
        }

        // If the OLD system roles map to a different NEW system role from the existing NEW system role,
        // then we need to remove the existing NEW system role and create the expected NEW system role for this User/Publication combination.
        return Changes(remove: existingNewSystemPublicationRole, create: expectedNewSystemPublicationRoleDetails);
    }

    private static NewPublicationRoleDetails? DetermineExpectedNewSystemPublicationRoleDetails(
        GroupedUserResourceRoles groupedUserResourceRoles
    )
    {
        var oldSystemPublicationRolesNewSystemEquivalents = groupedUserResourceRoles
            .PublicationRoles.Where(pr => !pr.Role.IsNewPermissionsSystemPublicationRole())
            .Select(pr => new NewPublicationRoleDetails(
                PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(pr.Role),
                pr.CreatedDate,
                pr.CreatedById,
                pr.EmailSent
            ))
            .ToHashSet();

        var releaseRolesNewSystemEquivalents = groupedUserResourceRoles
            .ReleaseRoles.Select(rr =>
                (
                    canConvertToNewPermissionsSystemPublicationRole: rr.Role.TryConvertToNewPermissionsSystemPublicationRole(
                        out var newSystemPublicationRole
                    ),
                    newSystemPublicationRole,
                    rr.CreatedDate,
                    rr.CreatedById,
                    rr.EmailSent
                )
            )
            .Where(tuple => tuple.canConvertToNewPermissionsSystemPublicationRole)
            .Select(tuple => new NewPublicationRoleDetails(
                tuple.newSystemPublicationRole!.Value,
                tuple.CreatedDate,
                tuple.CreatedById,
                tuple.EmailSent
            ))
            .ToHashSet();

        var allEquivalentNewSystemPublicationRoles = oldSystemPublicationRolesNewSystemEquivalents
            .Union(releaseRolesNewSystemEquivalents)
            .ToHashSet();

        // Pick the earliest Approver if exists, otherwise the earliest Drafter, otherwise null
        return allEquivalentNewSystemPublicationRoles
            .OrderByDescending(npr => npr.Role == PublicationRole.Approver) // Approver first, Drafter second
            .ThenBy(npr => npr.CreatedDate) // Then by earliest created date
            .FirstOrDefault();
    }

    private record ExistingPublicationRoleDetails(
        Guid Id,
        PublicationRole Role,
        DateTime CreatedDate,
        Guid? CreatedById,
        DateTimeOffset? EmailSent
    );

    private record ExistingReleaseRoleDetails(
        Guid Id,
        ReleaseRole Role,
        DateTime CreatedDate,
        Guid? CreatedById,
        DateTimeOffset? EmailSent
    );

    private record NewPublicationRoleDetails(
        PublicationRole Role,
        DateTime CreatedDate,
        Guid? CreatedById,
        DateTimeOffset? EmailSent
    );

    private record GroupedUserPublicationRoles(
        Guid UserId,
        Guid PublicationId,
        HashSet<ExistingPublicationRoleDetails> Roles
    );

    private record GroupedUserReleaseRoles(Guid UserId, Guid PublicationId, HashSet<ExistingReleaseRoleDetails> Roles);

    private record GroupedUserResourceRoles(
        Guid UserId,
        Guid PublicationId,
        HashSet<ExistingPublicationRoleDetails> PublicationRoles,
        HashSet<ExistingReleaseRoleDetails> ReleaseRoles
    );

    private record NewPermissionsSystemChanges(
        Guid UserId,
        Guid PublicationId,
        ExistingPublicationRoleDetails? NewSystemPublicationRoleToRemove,
        NewPublicationRoleDetails? NewSystemPublicationRoleToCreate
    );
}
