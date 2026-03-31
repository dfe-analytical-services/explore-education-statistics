#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublicationRoleChangesHelper : IPublicationRoleChangesHelper
{
    public (
        PublicationRole? publicationRoleToRemove,
        PublicationRole? publicationRoleToCreate
    ) DetermineChanges(
        PublicationRole? existingPublicationRoleForPublication,
        PublicationRole publicationRoleToCreate
    )
    {
        if (existingPublicationRoleForPublication is PublicationRole role &&
            !role.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{existingPublicationRoleForPublication}'. Expected a NEW permissions system role."
            );
        }

        if (!publicationRoleToCreate.IsNewPermissionsSystemPublicationRole())
        {
            throw new ArgumentException(
                $"Unexpected publication role: '{publicationRoleToCreate}'. Expected a NEW permissions system role."
            );
        }

        if (existingPublicationRoleForPublication == publicationRoleToCreate)
        {
            throw new ArgumentException(
                $"The publication role '{publicationRoleToCreate}' already exists."
            );
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
