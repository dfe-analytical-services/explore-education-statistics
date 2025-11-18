using System.Net.Http;
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
        _publicDataDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        _contentDbContext = _factory.Services.GetRequiredService<ContentDbContext>();
        _contentDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        _statisticsDbContext = _factory.Services.GetRequiredService<StatisticsDbContext>();
        _statisticsDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        _usersAndRolesDbContext = _factory.Services.GetRequiredService<UsersAndRolesDbContext>();
        _usersAndRolesDbContext.ChangeTracker.QueryTrackingBehavior =
            QueryTrackingBehavior.NoTracking;

        _processorClientMock = Mock.Get(_factory.Services.GetRequiredService<IProcessorClient>());
        _publicDataApiClientMock = Mock.Get(
            _factory.Services.GetRequiredService<IPublicDataApiClient>()
        );
    }

    public async Task DisposeAsync()
    {
        await _psql.Stop();
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
