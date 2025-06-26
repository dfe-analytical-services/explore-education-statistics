using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedHttpClientWithPsqlFixture : IAsyncLifetime
{
    private readonly OptimisedPostgreSqlContainerUtil _psql = new();
    private HttpClient _client;

    public Task InitializeAsync() => _psql.Start();

    public Task DisposeAsync() => _psql.Stop();
    
    public OptimisedPostgreSqlContainerUtil GetContainer() => _psql;
    
    public HttpClient GetClient()
    {
        if (_client != null)
        {
            return _client;
        }
        
        var factory = new TestWebApplicationFactory();

        _client = factory
            .ConfigureAdmin()
            .WithPostgres(_psql.GetContainer())
            .Build()
            .CreateClient();

        return _client;
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        
    }
}
