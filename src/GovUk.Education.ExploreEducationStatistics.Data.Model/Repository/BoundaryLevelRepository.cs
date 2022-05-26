#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class BoundaryLevelRepository : IBoundaryLevelRepository
    {
        private readonly StatisticsDbContext _context;

        public BoundaryLevelRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public Task<BoundaryLevel?> Get(long id)
        {
            return _context.BoundaryLevel.SingleOrDefaultAsync(level => level.Id == id);
        }

        public IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels)
        {
            return _context.BoundaryLevel
                .Where(level => geographicLevels.Contains(level.Level))
                .OrderByDescending(level => level.Published);
        }
    }
}
