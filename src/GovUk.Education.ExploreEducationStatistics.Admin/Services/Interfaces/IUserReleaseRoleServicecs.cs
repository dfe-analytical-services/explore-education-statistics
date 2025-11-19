#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserReleaseRoleService
{
    Task<List<UserReleaseRole>> ListLatestUserReleaseRolesByPublication(
        Guid publicationId,
        ReleaseRole[]? rolesToInclude,
        bool includeInactiveUsers = false
    );
}
