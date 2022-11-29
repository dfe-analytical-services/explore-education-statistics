using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(LocationCacheStartup), "LoadLocations")]
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor;

public class LocationCacheStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<LocationCacheLoader>();
    }

    [Extension("LoadLocations")]
    // ReSharper disable once ClassNeverInstantiated.Local
    private class LocationCacheLoader : IExtensionConfigProvider
    {
        private readonly IImporterLocationCache _importerLocationCache;
        private readonly IDbContextSupplier _contextSupplier;

        public LocationCacheLoader(
            IImporterLocationCache importerLocationCache, 
            IDbContextSupplier contextSupplier)
        {
            _importerLocationCache = importerLocationCache;
            _contextSupplier = contextSupplier;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var dbContext = _contextSupplier.CreateDbContext<StatisticsDbContext>();
            _importerLocationCache.LoadLocations(dbContext);
        }
    }
}