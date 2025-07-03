using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedSharedPostgresContainer : IAsyncDisposable
{
    private static readonly Lazy<OptimisedSharedPostgresContainer> StaticInstance = 
        new(() => new OptimisedSharedPostgresContainer());

    public static OptimisedSharedPostgresContainer Instance => StaticInstance.Value;

    private readonly PostgreSqlContainer _container;
    
    public string ConnectionString { get; private set; }

    private OptimisedSharedPostgresContainer()
    {
        _container = new OptimisedPostgreSqlContainerUtil().GetContainer();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
