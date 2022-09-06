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

        var publicBlobCacheService = GetBlobCacheService(scope.ServiceProvider, "PublicStorage");
        BlobCacheAttribute.AddService("public", publicBlobCacheService);
    }

    private static IBlobCacheService GetBlobCacheService(IServiceProvider provider, string connectionStringKey)
    {
        return new BlobCacheService(
            blobStorageService: GetBlobStorageService(provider, connectionStringKey),
            logger: provider.GetRequiredService<ILogger<BlobCacheService>>());
    }

    private static IBlobStorageService GetBlobStorageService(IServiceProvider provider, string connectionStringKey)
    {
        var connectionString = GetConfigurationValue(provider, connectionStringKey);
        return new BlobStorageService(
            connectionString,
            new BlobServiceClient(connectionString),
            provider.GetRequiredService<ILogger<BlobStorageService>>(),
            new StorageInstanceCreationUtil());
    }

    private static string GetConfigurationValue(IServiceProvider provider, string key)
    {
        var configuration = provider.GetService<IConfiguration>();
        return configuration.GetValue<string>(key);
    }
}
