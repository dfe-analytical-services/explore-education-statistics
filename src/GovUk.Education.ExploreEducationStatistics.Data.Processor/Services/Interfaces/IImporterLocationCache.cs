using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterLocationCache
{
    void LoadLocations(StatisticsDbContext context);
    
    Location Get(Location locationFromCsv);

    Task<Location> GetOrCreateAndCache(Location locationFromCsv, Func<Task<Location>> locationProvider);
}