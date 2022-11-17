using GovUk.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
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

        public LocationCacheLoader(IImporterLocationCache importerLocationCache)
        {
            _importerLocationCache = importerLocationCache;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            _importerLocationCache.LoadLocations(DbUtils.CreateStatisticsDbContext());
        }
    }
}