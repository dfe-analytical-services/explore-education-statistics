#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

// This class is only a temporary utility class used to migrate publication roles from the old
// permissions system to the new one. It will be removed in EES-6196.
public static class PublicationRoleUtils
{
    public static PublicationRole ConvertToNewPermissionsSystemPublicationRole(this PublicationRole publicationRole)
    {
        if (publicationRole.IsNewPermissionsSystemPublicationRole())
        {
            return publicationRole;
        }

        return publicationRole switch
        {
            PublicationRole.Owner => PublicationRole.Drafter,
            PublicationRole.Allower => PublicationRole.Approver,
            _ => throw new ArgumentOutOfRangeException(
                $"Unexpected publication role: '{publicationRole}'"),
        };
    }

    public static bool TryConvertToNewPermissionsSystemPublicationRole(this ReleaseRole oldSystemReleaseRole, [NotNullWhen(true)] out PublicationRole? newSystemPublicationRole)
    {
        switch (oldSystemReleaseRole)
        {
            case ReleaseRole.Contributor:
                newSystemPublicationRole = PublicationRole.Drafter;
                return true;
            case ReleaseRole.Approver:
                newSystemPublicationRole = PublicationRole.Approver;
                return true;
            default:
                newSystemPublicationRole = null;
                return false;
        }
    }

    public static bool IsNewPermissionsSystemPublicationRole(this PublicationRole publicationRole)
    {
        return publicationRole switch
        {
            PublicationRole.Drafter or PublicationRole.Approver => true,
            PublicationRole.Owner or PublicationRole.Allower => false,
            _ => throw new ArgumentOutOfRangeException(
                $"Unexpected publication role: '{publicationRole}'"),
        };
    }
}
