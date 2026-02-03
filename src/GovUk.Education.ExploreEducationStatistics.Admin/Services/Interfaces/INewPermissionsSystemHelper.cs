#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface INewPermissionsSystemHelper
{
    /// <summary>
    /// When using this method, it is assumed that each of the publication roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingPublicationRoles">All existing publication roles for a user/publication combination.</param>
    /// <param name="publicationRoleToCreate">The publication role to be created for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChanges(
        HashSet<PublicationRole> existingPublicationRoles,
        PublicationRole publicationRoleToCreate
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and the release role passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingPublicationRoles">All existing publication roles for a user/publication combination.</param>
    /// <param name="releaseRoleToCreate">The release role to be created for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChanges(
        HashSet<PublicationRole> existingPublicationRoles,
        ReleaseRole releaseRoleToCreate
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and release roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingPublicationRoles">All existing publication roles for a user/publication combination.</param>
    /// <param name="existingReleaseRoles">All existing release roles for a user/publication combination.</param>
    /// <param name="oldPublicationRoleToRemove">The OLD permissions system publication role to be deleted for the user/publication combination.</param>
    PublicationRole? DetermineNewPermissionsSystemRoleToRemove(
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole oldPublicationRoleToRemove
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and release roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingPublicationRoles">All existing publication roles for a user/publication combination.</param>
    /// <param name="existingReleaseRoles">All existing release roles for a user/publication combination.</param>
    /// <param name="releaseRoleToRemove">The release role to be deleted for the user/publication combination.</param>
    PublicationRole? DetermineNewPermissionsSystemRoleToRemove(
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        ReleaseRole releaseRoleToRemove
    );
}
