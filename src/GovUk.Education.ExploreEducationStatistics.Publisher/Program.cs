using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigurePublisherHostBuilder()
    .Build();

EnableCaching();

await host.RunAsync();
return;

void EnableCaching()
{
    // Enable caching and register any caching services
    CacheAspect.Enabled = true;
    BlobCacheAttribute.AddService(
        "public",
        new BlobCacheService(
            host.Services.GetRequiredService<IPublicBlobStorageService>(),
            host.Services.GetRequiredService<ILogger<BlobCacheService>>()
        )
    );
}
