using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedHttpClientWithPsqlFixture : IAsyncLifetime
{
    private readonly OptimisedPostgreSqlContainerUtil _psql = new();

    private WebApplicationFactory<Startup> _factory;
    private PublicDataDbContext _publicDataDbContext;
    private ContentDbContext _contentDbContext;
    private StatisticsDbContext _statisticsDbContext;
    private UsersAndRolesDbContext _usersAndRolesDbContext;
    private Mock<IProcessorClient> _processorClientMock;
    private Mock<IPublicDataApiClient> _publicDataApiClientMock;

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

        _processorClientMock = Mock.Get(_factory.Services.GetRequiredService<IProcessorClient>());
        _publicDataApiClientMock = Mock.Get(
            _factory.Services.GetRequiredService<IPublicDataApiClient>()
        );
    }

    /// <summary>
    /// This is called by the XUnit lifecycle management of test fixtures.
    /// Once the test suite has finished using this fixture (either at class or
    /// collection level), this method is called for us to dispose of any
    /// disposable resources that we are keeping handles on.
    ///
    /// For example, the reusable DbContexts that we use to seed test data
    /// and make assertions with are disposed of here, ensuring that they do
    /// not hang around and allows the full disposal of the
    /// WebApplicationFactory.
    /// </summary>
    public async Task DisposeAsync()
    {
        // await _psql.Stop();
        DisposeDbContexts();
    }

    public async Task ClearTestData()
    {
        _contentDbContext.ChangeTracker.Clear();
        await _contentDbContext.Database.EnsureDeletedAsync();

        _statisticsDbContext.ChangeTracker.Clear();
        await _statisticsDbContext.Database.EnsureDeletedAsync();

        _statisticsDbContext.ChangeTracker.Clear();
        await _statisticsDbContext.Database.EnsureDeletedAsync();

        await _publicDataDbContext.ClearTestData();
    }

    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    public void RegisterTestUser(ClaimsPrincipal user)
    {
        var userPool =
            _factory.Services.GetRequiredService<OptimisedWebApplicationFactoryBuilder<Startup>.TestUserPool>();

        userPool.AddUser(user);
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

    public Mock<IProcessorClient> GetProcessorClientMock()
    {
        return _processorClientMock;
    }

    public Mock<IPublicDataApiClient> GetPublicDataApiClientMock()
    {
        return _publicDataApiClientMock;
    }

    private void DisposeDbContexts()
    {
        _publicDataDbContext.Dispose();
        _contentDbContext.Dispose();
        _statisticsDbContext.Dispose();
        _usersAndRolesDbContext.Dispose();
    }

    private class TestWebApplicationFactory : WebApplicationFactory<Startup>;
}
