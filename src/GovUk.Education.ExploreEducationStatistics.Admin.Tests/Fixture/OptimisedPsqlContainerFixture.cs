using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedPsqlContainerFixture : IAsyncLifetime
{
    private readonly OptimisedPostgreSqlContainerUtil _psql = new();

    public Task InitializeAsync() => _psql.Start();

    public Task DisposeAsync() => _psql.Stop();
    
    public OptimisedPostgreSqlContainerUtil GetContainer() => _psql;
}
