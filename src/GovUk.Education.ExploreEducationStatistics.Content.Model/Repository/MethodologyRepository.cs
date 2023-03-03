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
    public class MethodologyRepository : IMethodologyRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public MethodologyRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<Methodology>> GetByPublication(Guid publicationId)
        {
            return await _contentDbContext.PublicationMethodologies
                .Where(pm => pm.PublicationId == publicationId)
                .Select(pm => pm.Methodology)
                .ToListAsync();
        }

        public async Task<Publication> GetOwningPublication(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .Include(mp => mp.Publications)
                .SingleAsync(mp => mp.Id == methodologyId);

            var owningPublicationLink = methodology.OwningPublication();

            await _contentDbContext.Entry(owningPublicationLink)
                .Reference(pm => pm.Publication)
                .LoadAsync();

            return owningPublicationLink.Publication;
        }

        public Task<List<Guid>> GetAllPublicationIds(Guid methodologyId)
        {
            return _contentDbContext.PublicationMethodologies
                .Where(pm => pm.MethodologyId == methodologyId)
                .Select(pm => pm.PublicationId)
                .ToListAsync();
        }

        public async Task<List<Methodology>> GetUnrelatedToPublication(Guid publicationId)
        {
            return await _contentDbContext.Methodologies
                .Include(m => m.Publications)
                .Where(m => m.Publications.All(pm => pm.PublicationId != publicationId))
                .ToListAsync();
        }
    }
}
