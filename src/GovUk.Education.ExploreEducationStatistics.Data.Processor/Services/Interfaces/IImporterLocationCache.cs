using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterLocationCache
{
    Location Find(Location locationFromCsv);

    Task<Location> GetOrCreateAndCacheAsync(Location locationFromCsv, Func<Task<Location>> locationProvider);
}