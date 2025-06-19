#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// This class will mostly likely be amended slightly in EES-6196, when we no longer have to cater for the old roles.
public class NewPermissionsSystemHelper(IUserPublicationRoleRepository userPublicationRoleRepository) : INewPermissionsSystemHelper
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
}
