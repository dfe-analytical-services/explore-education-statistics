#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

[Collection(CacheTestFixture.CollectionName)]
public abstract class OptimisedIntegrationTestFixture :
    CacheServiceTestFixture,
    IClassFixture<WebApplicationFactory<Startup>>,
    IClassFixture<CacheTestFixture>
{
    protected readonly DataFixture DataFixture = new();
}
