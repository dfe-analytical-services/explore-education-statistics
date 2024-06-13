using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

public abstract class FunctionsIntegrationTest<TFunctionsIntegrationTestFixture>(
    FunctionsIntegrationTestFixture fixture) :
    IClassFixture<TFunctionsIntegrationTestFixture>
    where TFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture
{
    protected readonly DataFixture DataFixture = new();
    private readonly IHost _host = fixture
        .ConfigureTestHostBuilder()
        .Build();

    protected async Task AddTestData<TDbContext>(Action<TDbContext> supplier) where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        supplier.Invoke(context);
        await context.SaveChangesAsync();
    }

    protected async Task EnsureDatabaseDeleted<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    protected TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = _host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    protected void ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        var context = GetDbContext<TDbContext>();
        context.ClearTestData();
    }

    protected void ResetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected TService GetRequiredService<TService>()
    {
        return _host.Services.GetRequiredService<TService>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class FunctionsIntegrationTestFixture
{
    public virtual IHostBuilder ConfigureTestHostBuilder()
    {
        return new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                hostContext.HostingEnvironment.EnvironmentName =
                    HostEnvironmentExtensions.IntegrationTestEnvironment;
                config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
            })
            .ConfigureWebHostDefaults(builder => builder.UseIntegrationTestEnvironment())
            .ConfigureServices(services =>
                GetFunctionTypes().ForEach(functionType => services.AddScoped(functionType)));
    }

    protected virtual IEnumerable<Type> GetFunctionTypes()
    {
        return [];
    }
}
