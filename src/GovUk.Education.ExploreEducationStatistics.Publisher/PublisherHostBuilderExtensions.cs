using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Configuration;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using IMethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IMethodologyService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.MethodologyService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Publisher;

public static class PublisherHostBuilderExtensions
{
    public static IHostBuilder ConfigurePublisherHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
            {
                configBuilder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .AddConfiguration(hostBuilderContext.Configuration);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var hostEnvironment = hostContext.HostingEnvironment;

                // TODO EES-5073 Remove this when the Public Data db exists in ALL Azure environments.
                var publicDataDbExists = configuration.GetValue<bool>("PublicDataDbExists");

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .Configure<JsonSerializerOptions>(options =>
                    {
                        // The 'IncludeFields' setting is necessary for the deserialization of the System.ValueTuple's used by
                        // the StageReleaseContentMessage and PublishReleaseFilesMessage queue message types.
                        // See https://github.com/dotnet/runtime/issues/70352
                        options.IncludeFields = true;
                    })
                    .AddMemoryCache()
                    .AddDbContext<ContentDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                            providerOptions => SqlServerDbContextOptionsBuilderExtensions.EnableCustomRetryOnFailure(providerOptions)))
                    .AddDbContext<StatisticsDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
                            providerOptions => providerOptions.EnableCustomRetryOnFailure()))
                    .AddSingleton<IFileStorageService, FileStorageService>(provider =>
                        new FileStorageService(configuration.GetValue<string>("PublisherStorage")))
                    .AddScoped<IPublishingService, PublishingService>()
                    .AddScoped<IPublicBlobStorageService, PublicBlobStorageService>()
                    .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>()
                    .AddScoped<IContentService, ContentService>(provider =>
                        new ContentService(
                            publicBlobStorageService: provider.GetRequiredService<IPublicBlobStorageService>(),
                            privateBlobCacheService: new BlobCacheService(
                                provider.GetRequiredService<IPrivateBlobStorageService>(),
                                provider.GetRequiredService<ILogger<BlobCacheService>>()),
                            publicBlobCacheService: new BlobCacheService(
                                provider.GetRequiredService<IPublicBlobStorageService>(),
                                provider.GetRequiredService<ILogger<BlobCacheService>>()),
                            releaseCacheService: provider.GetRequiredService<IReleaseCacheService>(),
                            releaseService: provider.GetRequiredService<IReleaseService>(),
                            methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                            publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()))
                    .AddScoped<IReleaseService, ReleaseService>()
                    .AddScoped<IPublisherTableStorageService, PublisherTableStorageService>()
                    .AddScoped<IMethodologyVersionRepository, MethodologyVersionRepository>()
                    .AddScoped<IMethodologyRepository, MethodologyRepository>()
                    .AddScoped<IMethodologyService, MethodologyService>()
                    .AddScoped<INotificationsService, NotificationsService>(provider =>
                        new NotificationsService(
                            context: provider.GetRequiredService<ContentDbContext>(),
                            storageQueueService: new StorageQueueService(
                                configuration.GetValue<string>("NotificationStorage"),
                                new StorageInstanceCreationUtil())))
                    .AddScoped<IQueueService, QueueService>(provider =>
                        new QueueService(
                            storageQueueService: new StorageQueueService(
                                configuration.GetValue<string>("PublisherStorage"),
                                new StorageInstanceCreationUtil()
                            ),
                            releasePublishingStatusService:
                            provider.GetRequiredService<IReleasePublishingStatusService>(),
                            logger: provider.GetRequiredService<ILogger<QueueService>>()))
                    .AddScoped<IReleasePublishingStatusService, ReleasePublishingStatusService>()
                    .AddScoped<IValidationService, ValidationService>()
                    .AddScoped<IFilterRepository, FilterRepository>()
                    .AddScoped<IFootnoteRepository, FootnoteRepository>()
                    .AddScoped<IIndicatorRepository, IndicatorRepository>()
                    .AddScoped<IPublishingCompletionService, PublishingCompletionService>()
                    .AddScoped<IPublicationRepository, PublicationRepository>()
                    .AddScoped<IReleaseVersionRepository, ReleaseVersionRepository>()
                    .AddScoped<IRedirectsCacheService, RedirectsCacheService>()
                    .AddScoped<IRedirectsService, RedirectsService>()
                    
                    .Configure<AppSettingOptions>(configuration.GetSection(AppSettingOptions.AppSettings));

                if (publicDataDbExists)
                {
                    services.AddScoped<IDataSetPublishingService, DataSetPublishingService>();
                }
                else
                {
                    services.AddScoped<IDataSetPublishingService, NoOpDataSetPublishingService>();
                }
                
                // TODO EES-3510 These services from the Content.Services namespace are used to update cached resources.
                // EES-3528 plans to send a request to the Content API to update its cached resources instead of this
                // being done from Publisher directly, and so these DI dependencies should eventually be removed.
                services
                    .AddAutoMapper(typeof(MappingProfiles))
                    // UserService is added to satisfy the IUserService dependency in Content.Services.ReleaseService
                    // even though it is not used
                    .AddScoped<IUserService, UserService>(_ => new UserService(null!, null!))
                    .AddScoped<GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService, GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService>()
                    .AddScoped<IMethodologyCacheService, MethodologyCacheService>()
                    .AddScoped<IPublicationService, PublicationService>()
                    .AddScoped<IPublicationCacheService, PublicationCacheService>()
                    .AddScoped<GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService, GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService>()
                    .AddScoped<IReleaseFileRepository, ReleaseFileRepository>()
                    .AddScoped<IReleaseCacheService, ReleaseCacheService>();

                // Only set up the `PublicDataDbContext` in non-integration test
                // environments. Otherwise, the connection string will be null and
                // cause the data source builder to throw a host exception.
                if (!hostEnvironment.IsIntegrationTest())
                {
                    var connectionString = configuration.GetConnectionString("PublicDataDb")!;

                    if (hostEnvironment.IsDevelopment())
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                        // Set up the data source outside the `AddDbContext` action as this
                        // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
                        // creating over 20 `IServiceProvider` instances.
                        var dbDataSource = dataSourceBuilder.Build();

                        services.AddDbContext<PublicDataDbContext>(options =>
                        {
                            options
                                .UseNpgsql(dbDataSource)
                                .EnableSensitiveDataLogging();
                        });
                    }
                    else
                    {
                        if (publicDataDbExists)
                        {
                            services.AddDbContext<PublicDataDbContext>(options =>
                            {
                                var sqlServerTokenProvider = new DefaultAzureCredential();
                                var accessToken = sqlServerTokenProvider.GetToken(
                                    new TokenRequestContext(scopes: new[]
                                    {
                                        "https://ossrdbms-aad.database.windows.net/.default"
                                    })).Token;

                                var connectionStringWithAccessToken =
                                    connectionString.Replace("[access_token]", accessToken);

                                var dbDataSource = new NpgsqlDataSourceBuilder(connectionStringWithAccessToken).Build();

                                options.UseNpgsql(dbDataSource);
                            });
                        }
                    }
                }

                StartupUtils.AddPersistenceHelper<ContentDbContext>(services);
                StartupUtils.AddPersistenceHelper<StatisticsDbContext>(services);
            });
    }

    // TODO EES-5073 Remove this when the Public Data db exists in ALL Azure environments.
    public class NoOpDataSetPublishingService : IDataSetPublishingService
    {
        public Task PublishDataSets(IEnumerable<Guid> releaseVersionIds)
        {
            return Task.CompletedTask;
        }
    }
}
