#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseRoleService : IUserReleaseRoleService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPublicationRepository _publicationRepository;

        public UserReleaseRoleService(ContentDbContext contentDbContext,
            IPublicationRepository publicationRepository)
        {
            _contentDbContext = contentDbContext;
            _publicationRepository = publicationRepository;
        }

        public async Task<List<UserReleaseRole>> ListUserReleaseRolesByPublication(ReleaseRole role,
            Guid publicationId)
        {
            var allLatestReleases = await _publicationRepository
                .ListActiveReleases(publicationId);

            var allLatestReleaseIds = allLatestReleases
                .Select(r => r.Id)
                .ToList();

            return await _contentDbContext.UserReleaseRoles
                .Include(releaseRole => releaseRole.User)
                .Where(userReleaseRole =>
                    allLatestReleaseIds.Contains(userReleaseRole.ReleaseId)
                    && userReleaseRole.Role == role)
                .ToListAsync();
        }
    }
}
