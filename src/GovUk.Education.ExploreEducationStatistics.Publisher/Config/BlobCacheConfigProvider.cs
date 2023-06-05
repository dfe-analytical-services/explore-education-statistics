#nullable enable
using System;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Config;

[Extension("BlobCache")]
// ReSharper disable once ClassNeverInstantiated.Global
public class BlobCacheConfigProvider : IExtensionConfigProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BlobCacheConfigProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Initialize(ExtensionConfigContext context)
    {
        // Enable caching and register any caching services.
        CacheAspect.Enabled = true;

        using var scope = _scopeFactory.CreateScope();

        var publicBlobCacheService = GetPublicBlobCacheService(scope.ServiceProvider);
        BlobCacheAttribute.AddService("public", publicBlobCacheService);
    }

    private static IBlobCacheService GetPublicBlobCacheService(IServiceProvider provider)
    {
        var configuration = provider.GetService<IConfiguration>();
        var publicConnectionString = configuration.GetValue<string>("PublicStorage");

        var publicBlobStorageService = new BlobStorageService(
            publicConnectionString,
            new BlobServiceClient(publicConnectionString),
            provider.GetRequiredService<ILogger<BlobStorageService>>(),
            new StorageInstanceCreationUtil());
        
        return new BlobCacheService(
            publicBlobStorageService,
            provider.GetRequiredService<ILogger<BlobCacheService>>());
    }
}
