#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleService : IUserReleaseRoleService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public UserReleaseRoleService(ContentDbContext contentDbContext,
        IReleaseVersionRepository releaseVersionRepository)
    {
        _contentDbContext = contentDbContext;
        _releaseVersionRepository = releaseVersionRepository;
    }

    public async Task<List<UserReleaseRole>> ListUserReleaseRolesByPublication(ReleaseRole role,
        Guid publicationId)
    {
        var releaseVersionIds = await _releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);
        return await _contentDbContext.UserReleaseRoles
            .Include(releaseRole => releaseRole.User)
            .Where(userReleaseRole =>
                releaseVersionIds.Contains(userReleaseRole.ReleaseVersionId)
                && userReleaseRole.Role == role)
            .ToListAsync();
    }
}
