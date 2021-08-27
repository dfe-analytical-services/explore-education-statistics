#nullable enable
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
            return await _contentDbContext.PublicationMethodologies
                .Where(pm => pm.PublicationId == publicationId)
                .Select(pm => pm.MethodologyParent)
                .ToListAsync();
        }

        public async Task<List<MethodologyParent>> GetUnrelatedToPublication(Guid publicationId)
        {
            return await _contentDbContext.MethodologyParents
                .Include(m => m.Publications)
                .Where(m => m.Publications.All(pm => pm.PublicationId != publicationId))
                .ToListAsync();
        }
    }
}
