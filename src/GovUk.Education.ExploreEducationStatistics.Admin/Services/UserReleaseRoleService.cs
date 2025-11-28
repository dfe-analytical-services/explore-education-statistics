#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleService(
    ContentDbContext contentDbContext,
    IReleaseVersionRepository releaseVersionRepository
) : IUserReleaseRoleService
{
    public async Task<List<UserReleaseRole>> ListLatestActiveUserReleaseRolesByPublication(
        Guid publicationId,
        params ReleaseRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<ReleaseRole>();

        var releaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);

        return await contentDbContext
            .UserReleaseRolesForActiveUsers.Where(urr => releaseVersionIds.Contains(urr.ReleaseVersionId))
            .WhereRolesIn(rolesToCheck)
            .Include(urr => urr.User)
            .ToListAsync();
    }
}
