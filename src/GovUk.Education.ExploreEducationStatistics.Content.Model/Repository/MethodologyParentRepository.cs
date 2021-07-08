using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class MethodologyParentRepository : IMethodologyParentRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public MethodologyParentRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<MethodologyParent>> GetByPublication(Guid publicationId)
        {
            // First check the publication exists
            var publication = await _contentDbContext.Publications.SingleAsync(p => p.Id == publicationId);

            return await _contentDbContext.MethodologyParents
                .Where(m => m.Publications.Any(pm => pm.PublicationId == publication.Id))
                .ToListAsync();
        }
    }
}
