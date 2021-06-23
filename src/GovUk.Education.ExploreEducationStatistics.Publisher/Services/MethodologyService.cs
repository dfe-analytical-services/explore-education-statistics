using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _context;

        public MethodologyService(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<Methodology> Get(Guid id)
        {
            return await _context.Methodologies.FindAsync(id);
        }

        public async Task<List<Methodology>> GetByRelease(Guid releaseId)
        {
            // TODO SOW4 EES-2385 Get the latest methodologies related to this release
            return await Task.FromResult(new List<Methodology>());
        }

        public async Task<List<File>> GetFiles(Guid methodologyId, params FileType[] types)
        {
            return await _context.MethodologyFiles
                .Include(mf => mf.File)
                .Where(mf => mf.MethodologyId == methodologyId)
                .Select(mf => mf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }
    }
}
