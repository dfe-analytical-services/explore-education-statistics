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

        public async Task<bool> UserHasReleaseRoleAssociatedWithMethodology(
            Guid userId,
            Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies.FindAsync(methodologyId);
            await _contentDbContext.Entry(methodology)
                .Collection(m => m.Publications)
                .LoadAsync();

            var viewablePublications = await GetViewablePublications(userId);

            return viewablePublications
                .Any(p1 => methodology.Publications
                    .Select(p2 => p2.Id)
                    .Contains(p1.Id));
        }

        public async Task<List<Methodology>> GetMethodologiesForUser(Guid userId)
        {
            var viewablePublications = await GetViewablePublications(userId);

            var allMethodologies = await _contentDbContext.Methodologies
                .Include(m => m.Publications)
                .ToListAsync();

            return allMethodologies
                .Where(m => viewablePublications
                    .Any(p1 => m.Publications
                        .Select(p2 => p2.Id)
                        .Contains(p1.Id)))
                .OrderBy(m => m.Title)
                .ToList();
        }

        private async Task<List<Publication>> GetViewablePublications(Guid userId)
        {
            return await _contentDbContext.UserReleaseRoles
                .Include(urr => urr.Release)
                .ThenInclude(r => r.Publication)
                .Where(urr =>
                    urr.UserId == userId
                    && urr.Role != ReleaseRole.PrereleaseViewer)
                .Select(urr => urr.Release.Publication)
                .Distinct()
                .ToListAsync();
        }
    }
}
