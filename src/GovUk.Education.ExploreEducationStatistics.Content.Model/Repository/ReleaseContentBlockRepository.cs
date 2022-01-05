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

        public async Task<List<T>> GetAll<T>(Guid releaseId) where T : ContentBlock
        {
            return await _contentDbContext
                .ReleaseContentBlocks
                .AsQueryable()
                .Where(releaseContentBlock => releaseContentBlock.ReleaseId == releaseId)
                .Select(releaseContentBlock => releaseContentBlock.ContentBlock)
                .OfType<T>()
                .ToListAsync();
        }
    }
}
