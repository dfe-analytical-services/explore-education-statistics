#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using IReleaseRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseRoleService : IUserReleaseRoleService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseRepository _releaseRepository;

        public UserReleaseRoleService(ContentDbContext contentDbContext,
            IReleaseRepository releaseRepository)
        {
            _contentDbContext = contentDbContext;
            _releaseRepository = releaseRepository;
        }

        public async Task<List<UserReleaseRole>> ListUserReleaseRolesByPublication(ReleaseRole role,
            Guid publicationId)
        {
            var releaseIds = await _releaseRepository.ListLatestReleaseVersionIds(publicationId);
            return await _contentDbContext.UserReleaseRoles
                .Include(releaseRole => releaseRole.User)
                .Where(userReleaseRole =>
                    releaseIds.Contains(userReleaseRole.ReleaseId)
                    && userReleaseRole.Role == role)
                .ToListAsync();
        }
    }
}
