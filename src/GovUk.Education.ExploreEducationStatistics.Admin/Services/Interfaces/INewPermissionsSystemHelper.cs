#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface INewPermissionsSystemHelper
{
    /// <summary>
    /// When using this method, it is assumed that each of the publication roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingSetOfPublicationRolesForPublication">All existing publication roles for a user/publication combination.</param>
    /// <param name="publicationRoleToCreate">The publication role to be created for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleCreation(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        PublicationRole publicationRoleToCreate
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and the release role passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingSetOfPublicationRolesForPublication">All existing publication roles for a user/publication combination.</param>
    /// <param name="releaseRoleToCreate">The release role to be created for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleCreation(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        ReleaseRole releaseRoleToCreate
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and release roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingSetOfPublicationRolesForPublication">All existing publication roles for a user/publication combination.</param>
    /// <param name="existingSetOfReleaseRolesForPublication">All existing release roles for a user/publication combination.</param>
    /// <param name="oldPublicationRoleToRemove">The OLD permissions system publication role to be deleted for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        HashSet<ReleaseRole> existingSetOfReleaseRolesForPublication,
        PublicationRole oldPublicationRoleToRemove
    );

    /// <summary>
    /// When using this method, it is assumed that each of the publication roles and release roles passed in are all related to the
    /// same user and publication.
    /// </summary>
    /// <param name="existingSetOfPublicationRolesForPublication">All existing publication roles for a user/publication combination.</param>
    /// <param name="allExistingReleaseRolesForPublication">All existing release roles for a user/publication combination.
    /// Do not pass a distinct set in here, as the method needs to work out if there are any roles of the target type
    /// remaining after removal. As we can have duplicates across different release versions, we need to be aware of them
    /// to be able to determine the expected outcome.</param>
    /// <param name="releaseRoleToRemove">The release role to be deleted for the user/publication combination.</param>
    (
        PublicationRole? newSystemPublicationRoleToRemove,
        PublicationRole? newSystemPublicationRoleToCreate
    ) DetermineNewPermissionsSystemChangesForRoleRemoval(
        HashSet<PublicationRole> existingSetOfPublicationRolesForPublication,
        IReadOnlyList<ReleaseRole> allExistingReleaseRolesForPublication,
        ReleaseRole releaseRoleToRemove
    );
}
