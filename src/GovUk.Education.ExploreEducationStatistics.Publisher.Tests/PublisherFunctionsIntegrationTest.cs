using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests;

public abstract class PublisherFunctionsIntegrationTest(PublisherFunctionsIntegrationTestFixture fixture)
    : FunctionsIntegrationTest<PublisherFunctionsIntegrationTestFixture>(fixture),
        IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        fixture.NotifierClient.Invocations.Clear();
        fixture.NotifierClient.Reset();

        fixture.EmailService.Invocations.Clear();
        fixture.EmailService.Reset();

        ResetDbContext<ContentDbContext>();
        await ClearTestData<PublicDataDbContext>();
    }

    public Task DisposeAsync()
    {
        return ClearAzureDataTableTestData(StorageConnectionString());
    }

    protected string StorageConnectionString()
    {
        return fixture.StorageConnectionString();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class PublisherFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    public readonly Mock<INotifierClient> NotifierClient = new(MockBehavior.Strict);

    public readonly Mock<IEmailService> EmailService = new(MockBehavior.Strict);

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:16.1-alpine").Build();

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder(
        "mcr.microsoft.com/azure-storage/azurite:3.35.0"
    )
        .WithInMemoryPersistence()
        .WithCommand("--skipApiVersionCheck")
        .Build();

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
        await _postgreSqlContainer.StartAsync();
    }

    public string StorageConnectionString()
    {
        return _azuriteContainer.GetConnectionString();
    }

    public override IHostBuilder ConfigureTestHostBuilder()
    {
        return base.ConfigureTestHostBuilder()
            .ConfigurePublisherHostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .AddJsonFile("appsettings.IntegrationTest.json", optional: false, reloadOnChange: false)
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            {
                                $"{AppOptions.Section}:{nameof(AppOptions.PrivateStorageConnectionString)}",
                                _azuriteContainer.GetConnectionString()
                            },
                            {
                                $"{AppOptions.Section}:{nameof(AppOptions.PublicStorageConnectionString)}",
                                _azuriteContainer.GetConnectionString()
                            },
                            {
                                $"{AppOptions.Section}:{nameof(AppOptions.NotifierStorageConnectionString)}",
                                _azuriteContainer.GetConnectionString()
                            },
                            {
                                $"{AppOptions.Section}:{nameof(AppOptions.PublisherStorageConnectionString)}",
                                _azuriteContainer.GetConnectionString()
                            },
                        }
                    )
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            { "DataFiles:BasePath", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) },
                        }
                    );
            })
            .ConfigureServices(
                (context, services) =>
                {
                    services.MockService<IPublicBlobCacheService>(MockBehavior.Loose);

                    services.ReplaceService(NotifierClient);

                    services.ReplaceService(EmailService);

                    services.UseInMemoryDbContext<ContentDbContext>(Guid.NewGuid().ToString());

                    services.AddDbContext<PublicDataDbContext>(options =>
                        options.UseNpgsql(
                            _postgreSqlContainer.GetConnectionString(),
                            psqlOptions => psqlOptions.EnableRetryOnFailure()
                        )
                    );

                    services.Configure<AppOptions>(context.Configuration.GetSection("App"));
                    services.Configure<NotifyOptions>(context.Configuration.GetSection("Notify"));

                    using var serviceScope = services
                        .BuildServiceProvider()
                        .GetRequiredService<IServiceScopeFactory>()
                        .CreateScope();

                    using var publicDataDbContext =
                        serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                    publicDataDbContext.Database.Migrate();
                }
            );
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return
        [
            typeof(NotifyChangeFunction),
            typeof(PublishMethodologyFilesFunction),
            typeof(PublishReleaseFilesFunction),
            typeof(PublishScheduledReleasesFunction),
            typeof(PublishTaxonomyFunction),
            typeof(StageScheduledReleasesFunction),
        ];
    }
}
