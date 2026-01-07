#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface INewPermissionsSystemHelper
{
    Task<(
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    )> DetermineNewPermissionsSystemChanges(PublicationRole publicationRoleToCreate, Guid userId, Guid publicationId);

    Task<(
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    )> DetermineNewPermissionsSystemChanges(ReleaseRole releaseRoleToCreate, Guid userId, Guid publicationId);

    Task<UserPublicationRole?> DetermineNewPermissionsSystemRoleToDelete(
        UserPublicationRole oldUserPublicationRoleToDelete
    );

    Task<UserPublicationRole?> DetermineNewPermissionsSystemRoleToDelete(UserReleaseRole userReleaseRoleToDelete);
}
