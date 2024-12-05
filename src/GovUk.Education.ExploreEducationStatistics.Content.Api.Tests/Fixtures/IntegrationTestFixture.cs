#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTestFixture(TestApplicationFactory testApp) :
    CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory>,
    IAsyncLifetime
{
    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp = testApp;

    public readonly double RandomGuid = new Random().Next(100);

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await TestApp.ClearAllTestData();
    }
}
