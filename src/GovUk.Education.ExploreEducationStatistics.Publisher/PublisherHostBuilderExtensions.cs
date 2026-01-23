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
using GovUk.Education.ExploreEducationStatistics.Events.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EducationInNumbersService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.EducationInNumbersService;
using IEducationInNumbersService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IEducationInNumbersService;
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
            .ConfigureAppConfiguration(
                (hostBuilderContext, configBuilder) =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddConfiguration(hostBuilderContext.Configuration);
                }
            )
            .ConfigureLogging(logging =>
            {
                // TODO EES-5013 Why can't this be controlled through application settings?
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .ConfigureServices();
    }

    public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(
            (hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var hostEnvironment = hostContext.HostingEnvironment;

                // TODO EES-5073 Remove this when the Public Data db exists in ALL Azure environments.
                var publicDataDbExists = configuration.GetValue<bool>("PublicDataDbExists");

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddMemoryCache()
                    .AddDbContext<ContentDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                            providerOptions =>
                                SqlServerDbContextOptionsBuilderExtensions.EnableCustomRetryOnFailure(providerOptions)
                        )
                    )
                    .AddDbContext<StatisticsDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
                            providerOptions => providerOptions.EnableCustomRetryOnFailure()
                        )
                    )
                    .AddSingleton(TimeProvider.System)
                    .AddSingleton<IFileStorageService, FileStorageService>(provider => new FileStorageService(
                        provider.GetRequiredService<IOptions<AppOptions>>().Value.PublisherStorageConnectionString
                    ))
                    .AddScoped<IPublishingService, PublishingService>()
                    .AddSingleton<IBlobSasService, BlobSasService>()
                    .AddScoped<IPublicBlobCacheService, PublicBlobCacheService>()
                    .AddScoped<IPublicBlobStorageService, PublicBlobStorageService>(
                        provider => new PublicBlobStorageService(
                            connectionString: provider
                                .GetRequiredService<IOptions<AppOptions>>()
                                .Value.PublicStorageConnectionString,
                            logger: provider.GetRequiredService<ILogger<IBlobStorageService>>(),
                            sasService: provider.GetRequiredService<IBlobSasService>()
                        )
                    )
                    .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>(
                        provider => new PrivateBlobStorageService(
                            connectionString: provider
                                .GetRequiredService<IOptions<AppOptions>>()
                                .Value.PrivateStorageConnectionString,
                            logger: provider.GetRequiredService<ILogger<IBlobStorageService>>(),
                            sasService: provider.GetRequiredService<IBlobSasService>()
                        )
                    )
                    .AddScoped<IContentService, ContentService>(provider => new ContentService(
                        contentDbContext: provider.GetRequiredService<ContentDbContext>(),
                        publicBlobStorageService: provider.GetRequiredService<IPublicBlobStorageService>(),
                        privateBlobCacheService: new BlobCacheService(
                            provider.GetRequiredService<IPrivateBlobStorageService>(),
                            provider.GetRequiredService<ILogger<BlobCacheService>>()
                        ),
                        publicBlobCacheService: new BlobCacheService(
                            provider.GetRequiredService<IPublicBlobStorageService>(),
                            provider.GetRequiredService<ILogger<BlobCacheService>>()
                        ),
                        releaseCacheService: provider.GetRequiredService<IReleaseCacheService>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                        publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()
                    ))
                    .AddScoped<IReleaseService, ReleaseService>()
                    .AddTransient<IPublisherTableStorageService, PublisherTableStorageService>(
                        provider => new PublisherTableStorageService(
                            provider.GetRequiredService<IOptions<AppOptions>>().Value.PublisherStorageConnectionString
                        )
                    )
                    .AddScoped<IMethodologyVersionRepository, MethodologyVersionRepository>()
                    .AddScoped<IMethodologyRepository, MethodologyRepository>()
                    .AddScoped<IMethodologyService, MethodologyService>()
                    .AddScoped<INotificationsService, NotificationsService>()
                    .AddScoped<IQueueService, QueueService>()
                    .AddScoped<IReleasePublishingStatusService, ReleasePublishingStatusService>()
                    .AddScoped<IValidationService, ValidationService>()
                    .AddScoped<IFilterRepository, FilterRepository>()
                    .AddScoped<IFootnoteRepository, FootnoteRepository>()
                    .AddScoped<IIndicatorRepository, IndicatorRepository>()
                    .AddScoped<IPublishingCompletionService, PublishingCompletionService>()
                    .AddScoped<IPublicationRepository, PublicationRepository>()
                    .AddScoped<IReleaseRepository, ReleaseRepository>()
                    .AddScoped<IReleaseVersionRepository, ReleaseVersionRepository>()
                    .AddScoped<IRedirectsCacheService, RedirectsCacheService>()
                    .AddScoped<IRedirectsService, RedirectsService>()
                    .AddScoped<IEducationInNumbersService, EducationInNumbersService>()
                    .AddEventGridClient(configuration)
                    .AddScoped<IPublisherEventRaiser, PublisherEventRaiser>()
                    .AddSingleton<INotifierClient, NotifierClient>(provider => new NotifierClient(
                        provider.GetRequiredService<IOptions<AppOptions>>().Value.NotifierStorageConnectionString
                    ))
                    .AddSingleton<IPublisherClient, PublisherClient>(provider => new PublisherClient(
                        provider.GetRequiredService<IOptions<AppOptions>>().Value.PublisherStorageConnectionString
                    ))
                    .AddSingleton<DateTimeProvider>()
                    .AddSingleton(TimeProvider.System)
                    .Configure<AppOptions>(configuration.GetSection(AppOptions.Section));

                // TODO EES-5073 Remove this check when the Public Data db is available in all Azure environments.
                if (publicDataDbExists)
                {
                    services
                        .AddOptions<DataFilesOptions>()
                        .Bind(configuration.GetRequiredSection(DataFilesOptions.Section));
                    services.AddScoped<IDataSetVersionPathResolver, DataSetVersionPathResolver>();
                    services.AddScoped<IDataSetPublishingService, DataSetPublishingService>();
                    services.AddScoped<IEducationInNumbersService, EducationInNumbersService>();
                }
                else
                {
                    services.AddScoped<IDataSetPublishingService, NoOpDataSetPublishingService>();
                    services.AddScoped<IEducationInNumbersService, NoOpEducationInNumbersService>();
                }

                // TODO EES-3510 These services from the Content.Services namespace are used to update cached resources.
                // EES-3528 plans to send a request to the Content API to update its cached resources instead of this
                // being done from Publisher directly, and so these DI dependencies should eventually be removed.
                services
                    .AddAutoMapper(typeof(MappingProfiles))
                    // UserService is added to satisfy the IUserService dependency in Content.Services.ReleaseService
                    // even though it is not used
                    .AddScoped<IUserService, UserService>(_ => new UserService(null!, null!))
                    .AddScoped<Content.Services.Interfaces.IMethodologyService, Content.Services.MethodologyService>()
                    .AddScoped<IMethodologyCacheService, MethodologyCacheService>()
                    .AddScoped<IPublicationService, PublicationService>()
                    .AddScoped<IPublicationCacheService, PublicationCacheService>()
                    .AddScoped<Content.Services.Interfaces.IReleaseService, Content.Services.ReleaseService>()
                    .AddScoped<IReleaseFileRepository, ReleaseFileRepository>()
                    .AddScoped<IReleaseCacheService, ReleaseCacheService>();

                // Only set up the `PublicDataDbContext` in non-integration test
                // environments. Otherwise, the connection string will be null and
                // cause the data source builder to throw a host exception.
                if (!hostEnvironment.IsIntegrationTest())
                {
                    if (publicDataDbExists)
                    {
                        var connectionString = ConnectionUtils.GetPostgreSqlConnectionString("PublicDataDb")!;
                        services.AddFunctionAppPsqlDbContext<PublicDataDbContext>(connectionString, hostContext);
                    }
                }

                StartupUtils.AddPersistenceHelper<ContentDbContext>(services);
                StartupUtils.AddPersistenceHelper<StatisticsDbContext>(services);
            }
        );
    }

    // TODO EES-5073 Remove this when the Public Data db exists in ALL Azure environments.
    public class NoOpDataSetPublishingService : IDataSetPublishingService
    {
        public Task PublishDataSets(Guid[] releaseVersionIds)
        {
            return Task.CompletedTask;
        }
    }

    public class NoOpEducationInNumbersService : IEducationInNumbersService
    {
        public Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate)
        {
            return Task.CompletedTask;
        }
    }
}
