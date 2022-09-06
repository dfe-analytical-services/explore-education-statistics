#nullable enable
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(BlobCacheStartup))]

namespace GovUk.Education.ExploreEducationStatistics.Publisher;

/// <summary>
/// <p>
/// Registering a BlobCacheService with BlobCacheAttribute can't take place in the default FunctionsStartup
/// Configure method as part of its own factory initialisation because that won't happen until the service is
/// required for dependency injection. A cached method could be invoked before it's been required by some other
/// component in which case it won't be initialised and it won't be registered with BlobCacheAttribute. If no
/// component requires a IBlobCacheService to be injected, it would never be initialised and registered.
/// </p>
///  <p>
/// It's also not possible in the FunctionsStartup Configure method to create a BlobCacheService or eagerly retrieve
/// one by getting it from the IServiceProvider because of its dependency on ILogger.
/// You can't request an ILogger during service initialisation because it's not set up yet.
/// See https://github.com/Azure/azure-functions-host/issues/7322
/// </p>
/// <p>
/// For these reasons, create and register any caching services using an extension. Their creation is independent of
/// whether they are required by DI and done at a stage during startup where logging is set up.
/// </p>
/// </summary>
public class BlobCacheStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<BlobCacheConfigProvider>();
    }
}
