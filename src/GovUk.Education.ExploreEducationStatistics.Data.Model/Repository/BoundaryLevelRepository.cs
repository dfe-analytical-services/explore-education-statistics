#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class BoundaryLevelRepository : IBoundaryLevelRepository
    {
        private readonly StatisticsDbContext _context;

        public BoundaryLevelRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BoundaryLevel>> Get()
        {
            return await _context.BoundaryLevel.ToListAsync();
        }

        public Task<BoundaryLevel?> Get(long id)
        {
            return id == 0
                ? throw new ArgumentNullException(nameof(id))
                : _context.BoundaryLevel.SingleOrDefaultAsync(level => level.Id == id);
        }

        public async Task Update(
            long id,
            string label)
        {
            if (id == 0)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var level = await _context.BoundaryLevel.FindAsync(id)
                ?? throw new KeyNotFoundException();

            level.Label = label;
            await _context.SaveChangesAsync();
        }

        public IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels)
        {
            return _context.BoundaryLevel
                .Where(level => geographicLevels.Contains(level.Level))
                .OrderByDescending(level => level.Published);
        }

        public async Task<BoundaryLevel> Create(
            GeographicLevel level,
            string label,
            DateTime published)
        {
            var newBoundaryLevel = await _context.BoundaryLevel.AddAsync(new()
            {
                Level = level,
                Label = label,
                Created = DateTime.UtcNow,
                Published = published,
            });

            await _context.SaveChangesAsync();

            return newBoundaryLevel.Entity;
        }
    }
}
