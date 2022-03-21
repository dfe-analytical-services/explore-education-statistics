using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class ReleaseContentSectionRepository : IReleaseContentSectionRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public ReleaseContentSectionRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<T>> GetAllContentBlocks<T>(Guid releaseId) where T : ContentBlock
        {
            return await _contentDbContext
                .ReleaseContentSections
                .AsQueryable()
                .Where(releaseContentSection => releaseContentSection.ReleaseId == releaseId)
                .Include(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Select(releaseContentSection => releaseContentSection.ContentSection)
                .SelectMany(section => section.Content)
                .OfType<T>()
                .ToListAsync();
        }
    }
}
