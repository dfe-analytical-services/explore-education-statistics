#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly StatisticsDbContext _context;

        public LocationRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<IList<Location>> GetDistinctForSubject(Guid subjectId)
        {
            return await _context
                .Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == subjectId)
                .Select(observation => observation.Location)
                .Distinct()
                .ToListAsync();
        }
    }
}
