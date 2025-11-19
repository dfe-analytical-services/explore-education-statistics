#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleService(
    ContentDbContext contentDbContext,
    IReleaseVersionRepository releaseVersionRepository
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

        var query = includeInactiveUsers ? contentDbContext.UserReleaseRoles : contentDbContext.ActiveUserReleaseRoles;

        return await query
            .Include(releaseRole => releaseRole.User)
            .Where(urr => releaseVersionIds.Contains(urr.ReleaseVersionId))
            .Where(urr => rolesToCheck.Contains(urr.Role))
            .ToListAsync();
    }
}
