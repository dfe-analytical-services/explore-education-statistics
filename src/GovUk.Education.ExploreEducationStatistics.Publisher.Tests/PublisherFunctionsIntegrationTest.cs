using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests;

[Collection(CacheTestFixture.CollectionName)]
public class PublisherFunctionsIntegrationTest : FunctionsIntegrationTest<PublisherFunctionsIntegrationTestFixture>
{
    protected PublisherFunctionsIntegrationTest(FunctionsIntegrationTestFixture fixture) : base(fixture)
    {
        ResetDbContext<ContentDbContext>();
        ClearTestData<PublicDataDbContext>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class PublisherFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }
    
    public override IHostBuilder ConfigureTestHostBuilder()
    {
        return base
            .ConfigureTestHostBuilder()
            .ConfigureHostBuilder()
            .ConfigureServices(services =>
            {
                services.UseInMemoryDbContext<ContentDbContext>();

                services.AddDbContext<PublicDataDbContext>(
                    options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

                using var serviceScope = services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();
            });
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return [
            typeof(NotifyChangeFunction),
            typeof(PublishImmediateReleaseContentFunction),
            typeof(PublishMethodologyFilesFunction),
            typeof(PublishReleaseFilesFunction),
            typeof(PublishScheduledReleasesFunction),
            typeof(PublishTaxonomyFunction),
            typeof(RetryReleasePublishingFunction),
            typeof(StageReleaseContentFunction),
            typeof(StageScheduledReleasesFunction)
        ];
    }
}
