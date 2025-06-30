#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// This class will mostly likely remain but be amended slightly in EES-6196, when we no longer have to cater for the old roles.
public class NewPermissionsSystemHelper(
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository) : INewPermissionsSystemHelper
{
    public async Task<(PublicationRole? newSystemPublicationRoleToRemove, PublicationRole? newSystemPublicationRoleToCreate)>
        DetermineNewPermissionsSystemChanges(PublicationRole publicationRoleToCreate, Guid userId, Guid publicationId)
    {
        var existingUserPublicationRoles = (await userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId))
            .ToHashSet();

        var newPermissionsSystemPublicationRole = PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(publicationRoleToCreate);

        return existingUserPublicationRoles.Contains(newPermissionsSystemPublicationRole)
            ? (null, null)
            : DetermineRolesToRemoveAndCreate(existingUserPublicationRoles, newPermissionsSystemPublicationRole);
    }

    public async Task<(PublicationRole? newSystemPublicationRoleToRemove, PublicationRole? newSystemPublicationRoleToCreate)>
        DetermineNewPermissionsSystemChanges(ReleaseRole releaseRoleToCreate, Guid userId, Guid publicationId)
    {
        var existingUserPublicationRoles = (await userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId))
            .ToHashSet();

        if (!releaseRoleToCreate.TryConvertToNewPermissionsSystemPublicationRole(out var newPermissionsSystemPublicationRole))
        {
            return (null, null);
        }

        return existingUserPublicationRoles.Contains(newPermissionsSystemPublicationRole.Value)
            ? (null, null)
            : DetermineRolesToRemoveAndCreate(existingUserPublicationRoles, newPermissionsSystemPublicationRole);
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public async Task<UserPublicationRole?> DetermineNewPermissionsSystemRoleToDelete(UserPublicationRole oldUserPublicationRoleToDelete)
    {
        if (oldUserPublicationRoleToDelete.Role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected OLD permissions system publication role: '{oldUserPublicationRoleToDelete.Role}'.");
        }

        var userId = oldUserPublicationRoleToDelete.UserId;
        var publicationId = oldUserPublicationRoleToDelete.PublicationId;

        var allUserPublicationRolesForPublication = await userPublicationRoleRepository
            .ListUserPublicationRolesByUserAndPublication(
                userId: userId,
                publicationId: publicationId);

        if (!allUserPublicationRolesForPublication.Any(upr => upr.Id == oldUserPublicationRoleToDelete.Id))
        {
            throw new ArgumentException($"User does not have the publication role '{oldUserPublicationRoleToDelete.Role}' assigned to the publication.");
        }

        var equivalentNewPermissionsSystemPublicationRoleToDelete = PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole(oldUserPublicationRoleToDelete.Role);

        var remainingPublicationRolesForPublication = allUserPublicationRolesForPublication
            .Where(upr => upr.Id != oldUserPublicationRoleToDelete.Id)
            .Select(upr => upr.Role)
            .ToHashSet();

        var allReleaseRolesForPublication = (await userReleaseRoleRepository
            .GetAllRolesByUserAndPublication(
                userId: userId,
                publicationId: publicationId))
            .ToHashSet();

        var shouldDeleteNewPermissionsSystemPublicationRole = ShouldDeleteNewPermissionsSystemPublicationRole(
            newPermissionsSystemPublicationRoleToCheck: equivalentNewPermissionsSystemPublicationRoleToDelete,
            remainingPublicationRoles: remainingPublicationRolesForPublication,
            remainingReleaseRoles: allReleaseRolesForPublication);

        return shouldDeleteNewPermissionsSystemPublicationRole
            ? await userPublicationRoleRepository.GetUserPublicationRole(
                userId: userId,
                publicationId: publicationId, 
                role: equivalentNewPermissionsSystemPublicationRoleToDelete)
            : null;
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    public async Task<UserPublicationRole?> DetermineNewPermissionsSystemRoleToDelete(UserReleaseRole userReleaseRoleToDelete)
    {
        var userId = userReleaseRoleToDelete.UserId;
        var publicationId = userReleaseRoleToDelete.ReleaseVersion.Release.PublicationId;

        var allUserReleaseRolesForPublication = await userReleaseRoleRepository
            .ListUserReleaseRolesByUserAndPublication(
                userId: userId,
                publicationId: publicationId);

        if (!allUserReleaseRolesForPublication.Any(urr => urr.Id == userReleaseRoleToDelete.Id))
        {
            throw new ArgumentException($"User does not have the publication role '{userReleaseRoleToDelete.Role}' assigned to the publication.");
        }

        if (!userReleaseRoleToDelete.Role.TryConvertToNewPermissionsSystemPublicationRole(out var equivalentNewPermissionsSystemPublicationRoleToDelete))
        {
            return null;
        }

        var remainingReleaseRolesForPublication = allUserReleaseRolesForPublication
            .Where(urr => urr.Id != userReleaseRoleToDelete.Id)
            .Select(urr => urr.Role)
            .ToHashSet();

        var allPublicationRolesForPublication = (await userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(
                userId: userId,
                publicationId: publicationId))
            .ToHashSet();

        var shouldDeleteNewPermissionsSystemPublicationRole = ShouldDeleteNewPermissionsSystemPublicationRole(
            newPermissionsSystemPublicationRoleToCheck: equivalentNewPermissionsSystemPublicationRoleToDelete.Value,
            remainingPublicationRoles: allPublicationRolesForPublication,
            remainingReleaseRoles: remainingReleaseRolesForPublication);

        return shouldDeleteNewPermissionsSystemPublicationRole
            ? await userPublicationRoleRepository.GetUserPublicationRole(
                userId: userId, 
                publicationId: publicationId, 
                role: equivalentNewPermissionsSystemPublicationRoleToDelete.Value)
            : null;
    }

    private static (PublicationRole? newSystemPublicationRoleToRemove, PublicationRole? newSystemPublicationRoleToCreate) 
        DetermineRolesToRemoveAndCreate(HashSet<PublicationRole> existingUserPublicationRoles, PublicationRole? newPermissionsSystemPublicationRole)
    {
        return newPermissionsSystemPublicationRole switch
        {
            PublicationRole.Approver when existingUserPublicationRoles.Contains(PublicationRole.Drafter)
                => (PublicationRole.Drafter, PublicationRole.Approver),

            PublicationRole.Approver
                => (null, PublicationRole.Approver),

            PublicationRole.Drafter when existingUserPublicationRoles.Contains(PublicationRole.Approver)
                => (null, null),

            PublicationRole.Drafter
                => (null, PublicationRole.Drafter),

            _ => throw new ArgumentException(
                $"Unexpected new permissions system publication role: '{newPermissionsSystemPublicationRole}'")
        };
    }

    // This particular method will be REMOVED in EES-6196, when we no longer have to cater for the old roles.
    private static bool ShouldDeleteNewPermissionsSystemPublicationRole(
        PublicationRole newPermissionsSystemPublicationRoleToCheck, 
        HashSet<PublicationRole> remainingPublicationRoles, 
        HashSet<ReleaseRole> remainingReleaseRoles)
    {
        var allEquivalentNewPermissionsSystemPublicationRoles = remainingPublicationRoles
            .Select(PublicationRoleUtils.ConvertToNewPermissionsSystemPublicationRole)
            .Union(
                remainingReleaseRoles
                    .Select(releaseRole => (
                        canConvertToNewPermissionsSystemPublicationRole: releaseRole.TryConvertToNewPermissionsSystemPublicationRole(out var newSystemPublicationRole), 
                        newSystemPublicationRole
                    ))
                    .Where(tuple => tuple.canConvertToNewPermissionsSystemPublicationRole)
                    .Select(tuple => tuple.newSystemPublicationRole!.Value))
            .ToHashSet();

        return !allEquivalentNewPermissionsSystemPublicationRoles.Contains(newPermissionsSystemPublicationRoleToCheck);
    }
}
