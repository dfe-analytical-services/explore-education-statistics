using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedHttpClientWithPsqlFixture : IAsyncLifetime
{
    private readonly OptimisedPostgreSqlContainerUtil _psql = new();
    // private HttpClient _client;
    
    private WebApplicationFactory<Startup> _factory;
    private PublicDataDbContext _publicDataDbContext;
    private ContentDbContext _contentDbContext;
    private StatisticsDbContext _statisticsDbContext;
    private UsersAndRolesDbContext _usersAndRolesDbContext;

    public async Task InitializeAsync()
    {
        await _psql.Start();
            
        _factory = new TestWebApplicationFactory()
            .ConfigureAdmin()
            .WithPostgres(_psql.GetContainer())
            .Build();

        _publicDataDbContext = _factory.Services.GetRequiredService<PublicDataDbContext>();
        _contentDbContext = _factory.Services.GetRequiredService<ContentDbContext>();
        _statisticsDbContext = _factory.Services.GetRequiredService<StatisticsDbContext>();
        _usersAndRolesDbContext = _factory.Services.GetRequiredService<UsersAndRolesDbContext>();
    }
    
    public void DisposeDbContexts()
    {
        _publicDataDbContext.Dispose();
        _contentDbContext.Dispose();
        _statisticsDbContext.Dispose();
        _usersAndRolesDbContext.Dispose();
    }

    public async Task DisposeAsync()
    {
        await _psql.Stop();
        DisposeDbContexts();
    }
    
    public OptimisedPostgreSqlContainerUtil GetContainer() => _psql;
    
    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    public WebApplicationFactory<Startup> GetFactory()
    {
        return _factory;
    }

    public ContentDbContext GetContentDbContext()
    {
        return _contentDbContext;
    }
    
    public StatisticsDbContext GetStatisticsDbContext()
    {
        return _statisticsDbContext;
    }
    
    public UsersAndRolesDbContext GetUsersAndRolesDbContext()
    {
        return _usersAndRolesDbContext;
    }
    
    public PublicDataDbContext GetPublicDataDbContext()
    {
        return _publicDataDbContext;
    }
    
    private class TestWebApplicationFactory : WebApplicationFactory<Startup>;
}
