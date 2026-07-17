#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublicationRoleChangesHelper : IPublicationRoleChangesHelper
{
    public (PublicationRole? publicationRoleToRemove, PublicationRole? publicationRoleToCreate) DetermineChanges(
        PublicationRole? existingPublicationRoleForPublication,
        PublicationRole publicationRoleToCreate
    )
    {
        if (existingPublicationRoleForPublication == publicationRoleToCreate)
        {
            throw new ArgumentException($"The publication role '{publicationRoleToCreate}' already exists.");
        }

        if (!existingPublicationRoleForPublication.HasValue)
        {
            return (null, publicationRoleToCreate);
        }

        if (existingPublicationRoleForPublication.Value is PublicationRole.Approver)
        {
            return (null, null);
        }

        if (publicationRoleToCreate == PublicationRole.Approver)
        {
            return (PublicationRole.Drafter, PublicationRole.Approver);
        }

        return (null, null);
    }
}
