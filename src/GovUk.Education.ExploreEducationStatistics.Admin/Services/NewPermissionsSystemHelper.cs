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
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole oldPublicationRoleToCreate
    )
    {
        if (oldPublicationRoleToCreate.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{oldPublicationRoleToCreate}'. Expected an OLD permissions system role."
            );
        }

        if (existingPublicationRoles.Contains(oldPublicationRoleToCreate))
        {
            throw new ArgumentException(
                $"The publication role '{oldPublicationRoleToCreate}' is already in the existing list of publication roles."
            );
        }

        var effectiveSetOfResultantPublicationRoles = existingPublicationRoles
            .Append(oldPublicationRoleToCreate)
            .ToHashSet();

        return DetermineRolesToRemoveAndCreate(effectiveSetOfResultantPublicationRoles, existingReleaseRoles);
    }

    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleCreation(
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        ReleaseRole releaseRoleToCreate
    )
    {
        if (existingReleaseRoles.Contains(releaseRoleToCreate))
        {
            throw new ArgumentException(
                $"The release role '{releaseRoleToCreate}' is already in the existing list of release roles."
            );
        }

        var effectiveSetOfResultantReleaseRoles = existingReleaseRoles.Append(releaseRoleToCreate).ToHashSet();

        return DetermineRolesToRemoveAndCreate(existingPublicationRoles, effectiveSetOfResultantReleaseRoles);
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
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

        var effectiveSetOfResultantPublicationRoles = existingPublicationRoles
            .Except([oldPublicationRoleToRemove])
            .ToHashSet();

        return DetermineRolesToRemoveAndCreate(effectiveSetOfResultantPublicationRoles, existingReleaseRoles);
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
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

        var effectiveSetOfResultantReleaseRoles = existingReleaseRoles.Except([releaseRoleToRemove]).ToHashSet();

        return DetermineRolesToRemoveAndCreate(existingPublicationRoles, effectiveSetOfResultantReleaseRoles);
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
