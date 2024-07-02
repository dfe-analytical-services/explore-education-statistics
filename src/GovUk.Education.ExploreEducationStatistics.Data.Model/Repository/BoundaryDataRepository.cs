#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
public class BoundaryDataRepository : IBoundaryDataRepository
{
    private readonly StatisticsDbContext _context;

    public BoundaryDataRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task AddRange(List<BoundaryData> boundaryData)
    {
        await _context.BoundaryData.AddRangeAsync(boundaryData);
        await _context.SaveChangesAsync();
    }
}
