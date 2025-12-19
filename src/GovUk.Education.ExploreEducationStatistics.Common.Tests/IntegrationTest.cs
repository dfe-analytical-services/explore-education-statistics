using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

public abstract class IntegrationTest<TStartup>(TestApplicationFactory<TStartup> testApp)
    : IClassFixture<TestApplicationFactory<TStartup>>
    where TStartup : class
{
    protected readonly TestApplicationFactory<TStartup> TestApp = testApp;
}
