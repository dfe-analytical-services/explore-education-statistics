#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// The method in this class will remain but be moved back to the 'UserReleaseRoleRespository' class in EES-6196, when we no longer
// have to cater for the old roles. For now, it have been moved here temporarily for convenience to allow
// us to avoid duplication, and avoid using circular dependencies where the two resource role repositories would depend on each other.
// An interface has purposefully NOT been created, as I do not want to mock this dependency. It should be tested as part of the
// corresponding repository tests. As this is a temporary class, we do not want to be moving/switching tests around just to move them back again later.
public class UserReleaseRoleQueryRepository(ContentDbContext contentDbContext)
{
    public IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly) =>
        resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => contentDbContext.UserReleaseRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => contentDbContext.UserReleaseRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => contentDbContext.UserReleaseRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => contentDbContext.UserReleaseRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
}
