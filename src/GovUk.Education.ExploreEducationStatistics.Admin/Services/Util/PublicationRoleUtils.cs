#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

// This class is only a temporary utility class. It will be removed once the old permission roles have been removed in EES-6212
public static class PublicationRoleUtils
{
    public static bool IsNewPermissionsSystemPublicationRole(this PublicationRole publicationRole)
    {
        return publicationRole switch
        {
            PublicationRole.Drafter or PublicationRole.Approver => true,
            PublicationRole.Owner or PublicationRole.Allower => false,
            _ => throw new ArgumentOutOfRangeException($"Unexpected publication role: '{publicationRole}'"),
        };
    }
}
