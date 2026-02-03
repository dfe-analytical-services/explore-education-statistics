#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using LinqToDB;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// This class will be amended slightly in EES-6196, when we no longer have to cater for the old roles.
// I envisage that the 'DetermineNewPermissionsSystemChanges' logic will be changed/moved into the UserPublicationRoleRepository, and
// the 'DetermineNewPermissionsSystemRoleToDelete' method will be removed entirely. The reason the former will still be required,
// is to assist in determining when a NEW permissions system publication role will need upgrading from Drafter to Approver.
public class NewPermissionsSystemHelper : INewPermissionsSystemHelper
{
    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChanges(
        HashSet<PublicationRole> existingPublicationRoles,
        PublicationRole publicationRoleToCreate
    )
    {
        var publicationRoleToCreateNewSystemEquivalent =
            PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(publicationRoleToCreate);

        return DetermineRolesToRemoveAndCreate(existingPublicationRoles, publicationRoleToCreateNewSystemEquivalent);
    }

    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChanges(
        HashSet<PublicationRole> existingPublicationRoles,
        ReleaseRole releaseRoleToCreate
    )
    {
        if (
            !releaseRoleToCreate.TryConvertToNewPermissionsSystemPublicationRole(
                out var releaseRoleToCreateNewSystemEquivalent
            )
        )
        {
            return (null, null);
        }

        return DetermineRolesToRemoveAndCreate(existingPublicationRoles, releaseRoleToCreateNewSystemEquivalent.Value);
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public PublicationRole? DetermineNewPermissionsSystemRoleToRemove(
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole oldPublicationRoleToRemove
    )
    {
        if (oldPublicationRoleToRemove.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{oldPublicationRoleToRemove}'. Expected an OLD permissions system role."
            );
        }

        if (!existingPublicationRoles.Contains(oldPublicationRoleToRemove))
        {
            throw new ArgumentException(
                $"The publication role '{oldPublicationRoleToRemove}' is not in the existing list of publication roles."
            );
        }

        var equivalentNewPermissionsSystemPublicationRoleToDelete =
            PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(oldPublicationRoleToRemove);

        if (!existingPublicationRoles.Contains(equivalentNewPermissionsSystemPublicationRoleToDelete))
        {
            return null;
        }

        var remainingPublicationRoles = existingPublicationRoles.Except([oldPublicationRoleToRemove]).ToHashSet();

        var shouldRemoveNewPermissionsSystemPublicationRole = ShouldRemoveNewPermissionsSystemPublicationRole(
            newPermissionsSystemPublicationRoleToCheck: equivalentNewPermissionsSystemPublicationRoleToDelete,
            remainingPublicationRoles: remainingPublicationRoles,
            remainingReleaseRoles: existingReleaseRoles
        );

        return shouldRemoveNewPermissionsSystemPublicationRole
            ? equivalentNewPermissionsSystemPublicationRoleToDelete
            : null;
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public PublicationRole? DetermineNewPermissionsSystemRoleToRemove(
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        ReleaseRole releaseRoleToRemove
    )
    {
        if (!existingReleaseRoles.Contains(releaseRoleToRemove))
        {
            throw new ArgumentException(
                $"The release role '{releaseRoleToRemove}' is not in the existing list of release roles."
            );
        }

        if (
            !releaseRoleToRemove.TryConvertToNewPermissionsSystemPublicationRole(
                out var equivalentNewPermissionsSystemPublicationRoleToDelete
            )
        )
        {
            return null;
        }

        if (!existingPublicationRoles.Contains(equivalentNewPermissionsSystemPublicationRoleToDelete.Value))
        {
            return null;
        }

        var remainingReleaseRoles = existingReleaseRoles.Except([releaseRoleToRemove]).ToHashSet();

        var shouldRemoveNewPermissionsSystemPublicationRole = ShouldRemoveNewPermissionsSystemPublicationRole(
            newPermissionsSystemPublicationRoleToCheck: equivalentNewPermissionsSystemPublicationRoleToDelete.Value,
            remainingPublicationRoles: existingPublicationRoles,
            remainingReleaseRoles: remainingReleaseRoles
        );

        return shouldRemoveNewPermissionsSystemPublicationRole
            ? equivalentNewPermissionsSystemPublicationRoleToDelete.Value
            : null;
    }

    private static (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineRolesToRemoveAndCreate(
        HashSet<PublicationRole> existingPublicationRoles,
        PublicationRole resourceRoleToCreateNewSystemEquivalent
    )
    {
        if (!resourceRoleToCreateNewSystemEquivalent.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{resourceRoleToCreateNewSystemEquivalent}'. Only NEW permissions system roles should be passed into this method."
            );
        }

        if (existingPublicationRoles.Contains(resourceRoleToCreateNewSystemEquivalent))
        {
            return (null, null);
        }

        return resourceRoleToCreateNewSystemEquivalent switch
        {
            PublicationRole.Approver when existingPublicationRoles.Contains(PublicationRole.Drafter) => (
                PublicationRole.Drafter,
                PublicationRole.Approver
            ),

            PublicationRole.Approver => (null, PublicationRole.Approver),

            PublicationRole.Drafter when existingPublicationRoles.Contains(PublicationRole.Approver) => (null, null),

            PublicationRole.Drafter => (null, PublicationRole.Drafter),

            _ => throw new ArgumentException(
                $"Unexpected new permissions system publication role: '{resourceRoleToCreateNewSystemEquivalent}'"
            ),
        };
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    private static bool ShouldRemoveNewPermissionsSystemPublicationRole(
        PublicationRole newPermissionsSystemPublicationRoleToCheck,
        HashSet<PublicationRole> remainingPublicationRoles,
        HashSet<ReleaseRole> remainingReleaseRoles
    )
    {
        var remainingOldPublicationRoles = remainingPublicationRoles
            .Where(role => !role.IsNewPermissionsSystemPublicationRole())
            .ToHashSet();

        var allEquivalentNewPermissionsSystemPublicationRoles = remainingOldPublicationRoles
            .Select(PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole)
            .Union(
                remainingReleaseRoles
                    .Select(releaseRole =>
                        (
                            canConvertToNewPermissionsSystemPublicationRole: releaseRole.TryConvertToNewPermissionsSystemPublicationRole(
                                out var newSystemPublicationRole
                            ),
                            newSystemPublicationRole
                        )
                    )
                    .Where(tuple => tuple.canConvertToNewPermissionsSystemPublicationRole)
                    .Select(tuple => tuple.newSystemPublicationRole!.Value)
            )
            .ToHashSet();

        return !allEquivalentNewPermissionsSystemPublicationRoles.Contains(newPermissionsSystemPublicationRoleToCheck);
    }
}
