using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyRepository : IMethodologyRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public MethodologyRepository(
            ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<Methodology>> GetMethodologiesForUser(Guid userId)
        {
            return await _contentDbContext.UserReleaseRoles
                .Include(urr => urr.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Methodology)
                .Where(urr =>
                    urr.UserId == userId
                    && urr.Release.Publication.MethodologyId != null
                    && urr.Role != ReleaseRole.PrereleaseViewer)
                .Select(urr => urr.Release.Publication.Methodology)
                .Distinct()
                .ToListAsync();
        }
    }
}
