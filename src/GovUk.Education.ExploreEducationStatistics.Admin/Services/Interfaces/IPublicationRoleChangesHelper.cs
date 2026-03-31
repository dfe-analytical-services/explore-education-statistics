#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublicationRoleChangesHelper
{
    /// <summary>
    /// When using this method, it is assumed that we are in the context of a particular user/publication combination.
    /// </summary>
    /// <param name="existingPublicationRoleForPublication">The existing publication role for the user/publication combination, if one exists.</param>
    /// <param name="publicationRoleToCreate">The publication role to be created for the user/publication combination.</param>
    (
        PublicationRole? publicationRoleToRemove,
        PublicationRole? publicationRoleToCreate
    ) DetermineChanges(
        PublicationRole? existingPublicationRoleForPublication,
        PublicationRole publicationRoleToCreate
    );
}
