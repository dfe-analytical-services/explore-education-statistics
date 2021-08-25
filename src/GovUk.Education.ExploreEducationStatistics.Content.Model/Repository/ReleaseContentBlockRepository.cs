using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class ReleaseContentBlockRepository : IReleaseContentBlockRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public ReleaseContentBlockRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<T>> GetByRelease<T>(Guid releaseId) where T : ContentBlock
        {
            return await _contentDbContext
                .ContentBlocks
                .Where(contentBlock => contentBlock.ReleaseId == releaseId)
                .OfType<T>()
                .ToListAsync();
        }
    }
}
