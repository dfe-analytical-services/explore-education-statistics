#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleService(
    IReleaseVersionRepository releaseVersionRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository
) : IUserReleaseRoleService
{
    public async Task<List<UserReleaseRole>> ListLatestUserReleaseRolesByPublication(
        Guid publicationId,
        ReleaseRole[]? rolesToInclude,
        bool includeInactiveUsers = false
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        var releaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);

        return await userReleaseRoleRepository
            .Query(includeInactiveUsers: includeInactiveUsers)
            .Where(urr => releaseVersionIds.Contains(urr.ReleaseVersionId))
            .WhereRolesIn(rolesToCheck)
            .Include(urr => urr.User)
            .ToListAsync();
    }
}
