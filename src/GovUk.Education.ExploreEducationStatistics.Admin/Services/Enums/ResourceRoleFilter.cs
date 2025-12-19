using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;

/// <summary>
/// Represents the possible statuses for a resource role (<see cref="UserPublicationRole"/>/<see cref="UserReleaseRole"/>).
/// </summary>
public enum ResourceRoleFilter
{
    /// <summary>
    /// Only include roles associated with ACTIVE Users.
    /// </summary>
    ActiveOnly,

    /// <summary>
    /// Only include roles associated with Users with PENDING invites
    /// </summary>
    PendingOnly,

    /// <summary>
    /// Include all roles EXCEPT those associated with Users with EXPIRED invites.
    /// </summary>
    AllButExpired,

    /// <summary>
    /// Include all roles, regardless of status.
    /// </summary>
    All,
}
