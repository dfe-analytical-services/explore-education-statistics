#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

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
    ) DetermineNewPermissionsSystemChangesForRoleCreation(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        PublicationRole oldPublicationRoleToCreate
    )
    {
        if (oldPublicationRoleToCreate.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{oldPublicationRoleToCreate}'. Expected an OLD permissions system role."
            );
        }

        if (existingSetOfPublicationRolesForPublication.Contains(oldPublicationRoleToCreate))
        {
            throw new ArgumentException(
                $"The publication role '{oldPublicationRoleToCreate}' is already in the existing set of publication roles."
            );
        }

        var effectiveSetOfResultantPublicationRoles = existingSetOfPublicationRolesForPublication
            .Append(oldPublicationRoleToCreate)
            .ToHashSet();

        return DetermineRolesToRemoveAndCreate(
            effectiveSetOfResultantPublicationRoles,
            existingSetOfReleaseRolesForPublication
        );
    }

    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleCreation(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        ReleaseRole releaseRoleToCreate
    )
    {
        var effectiveSetOfResultantReleaseRoles = existingSetOfReleaseRolesForPublication
            .Append(releaseRoleToCreate)
            .ToHashSet();

        return DetermineRolesToRemoveAndCreate(
            existingSetOfPublicationRolesForPublication,
            effectiveSetOfResultantReleaseRoles
        );
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        PublicationRole oldPublicationRoleToRemove
    )
    {
        if (oldPublicationRoleToRemove.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{oldPublicationRoleToRemove}'. Expected an OLD permissions system role."
            );
        }

        if (!existingSetOfPublicationRolesForPublication.Contains(oldPublicationRoleToRemove))
        {
            throw new ArgumentException(
                $"The publication role '{oldPublicationRoleToRemove}' is not in the existing set of publication roles."
            );
        }

        var effectiveSetOfResultantPublicationRoles = existingSetOfPublicationRolesForPublication
            .Except([oldPublicationRoleToRemove])
            .ToHashSet();

        return DetermineRolesToRemoveAndCreate(
            effectiveSetOfResultantPublicationRoles,
            existingSetOfReleaseRolesForPublication
        );
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        IReadOnlyList<ReleaseRole> allExistingReleaseRolesForPublication,
        ReleaseRole releaseRoleToRemove
    )
    {
        var effectiveListOfResultantReleaseRoles = allExistingReleaseRolesForPublication.ToList();

        if (!effectiveListOfResultantReleaseRoles.Remove(releaseRoleToRemove))
        {
            throw new ArgumentException(
                $"The release role '{releaseRoleToRemove}' is not in the existing list of release roles."
            );
        }

        var effectiveSetOfResultantReleaseRoles = effectiveListOfResultantReleaseRoles.ToHashSet();

        return DetermineRolesToRemoveAndCreate(
            existingSetOfPublicationRolesForPublication,
            effectiveSetOfResultantReleaseRoles
        );
    }

    private static (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineRolesToRemoveAndCreate(
        HashSet<PublicationRole> effectiveSetOfResultantPublicationRoles,
        HashSet<ReleaseRole> effectiveSetOfResultantReleaseRoles
    )
    {
        var equivalentNewRoles = effectiveSetOfResultantPublicationRoles
            .Where(r => !r.IsNewPermissionsSystemPublicationRole())
            .Select(PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole)
            .Concat(
                effectiveSetOfResultantReleaseRoles
                    .Select(r => r.TryConvertToNewPermissionsSystemPublicationRole(out var role) ? role : null)
                    .Where(r => r.HasValue)
                    .Select(r => r!.Value)
            )
            .ToHashSet();

        PublicationRole? expectedNewSystemPublicationRole =
            equivalentNewRoles.Contains(PublicationRole.Approver) ? PublicationRole.Approver
            : equivalentNewRoles.Contains(PublicationRole.Drafter) ? PublicationRole.Drafter
            : null;

        var hasApprover = effectiveSetOfResultantPublicationRoles.Contains(PublicationRole.Approver);
        var hasDrafter = effectiveSetOfResultantPublicationRoles.Contains(PublicationRole.Drafter);

        if (expectedNewSystemPublicationRole is null)
        {
            return hasApprover ? (PublicationRole.Approver, null)
                : hasDrafter ? (PublicationRole.Drafter, null)
                : (null, null);
        }

        if (expectedNewSystemPublicationRole is PublicationRole.Approver)
        {
            if (hasApprover)
            {
                return (null, null);
            }

            if (hasDrafter)
            {
                return (PublicationRole.Drafter, PublicationRole.Approver);
            }

            return (null, PublicationRole.Approver);
        }

        if (expectedNewSystemPublicationRole is PublicationRole.Drafter)
        {
            if (hasDrafter)
            {
                return (null, null);
            }

            if (hasApprover)
            {
                return (PublicationRole.Approver, PublicationRole.Drafter);
            }

            return (null, PublicationRole.Drafter);
        }

        throw new ArgumentException($"Unexpected role: {expectedNewSystemPublicationRole}");
    }
}
