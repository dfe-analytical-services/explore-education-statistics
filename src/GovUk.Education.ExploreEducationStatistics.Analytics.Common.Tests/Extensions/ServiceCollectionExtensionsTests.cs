using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(false, typeof(NoOpAnalyticsManager))]
    [InlineData(true, typeof(AnalyticsManager))]
    public void GivenAddAnalyticsCommonCalled_WhenAnalyticsIsEnabled_ThenAnalyticsManagerIsResolved(bool isAnalyticsEnabled, Type expectedAnalyticsManagerImplementation)
    {
        // Arrange
        IServiceCollection serviceCollection = new ServiceCollection();
        
        // Act
        serviceCollection = serviceCollection.AddAnalyticsCommon(isAnalyticsEnabled).Services;
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var actual = serviceProvider.GetRequiredService<IAnalyticsManager>();
        Assert.IsType(expectedAnalyticsManagerImplementation, actual);
    }
    
    [Fact]
    public void GivenAnalyticsIsEnabled_WhenAnalyticsWritersAdded_ThenAnalyticsWritersResolved()
    {
        // Arrange
        IServiceCollection serviceCollection = new ServiceCollection();
        
        // Act
        serviceCollection = serviceCollection
            .AddAnalyticsCommon(isAnalyticsEnabled:true)
            .AddWriteStrategy<TestAnalyticsWriter1>()
            .AddWriteStrategy<TestAnalyticsWriter2>()
            .Services;
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var actual = serviceProvider.GetRequiredService<IEnumerable<IAnalyticsWriteStrategy>>();
        Assert.NotNull(actual);
        var actualTypes = actual.Select(instance => instance.GetType()).ToArray();
        Assert.Equal(2, actualTypes.Length);
        Assert.Contains(typeof(TestAnalyticsWriter1), actualTypes);
        Assert.Contains(typeof(TestAnalyticsWriter2), actualTypes);
    }
    
    [Fact]
    public void GivenAnalyticsIsNotEnabled_WhenAnalyticsWritersAdded_ThenNoAnalyticsWritersResolved()
    {
        // Arrange
        IServiceCollection serviceCollection = new ServiceCollection();
        
        // Act
        serviceCollection = serviceCollection
            .AddAnalyticsCommon(isAnalyticsEnabled:false)
            .AddWriteStrategy<TestAnalyticsWriter1>()
            .AddWriteStrategy<TestAnalyticsWriter2>()
            .Services;
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var actual = serviceProvider.GetRequiredService<IEnumerable<IAnalyticsWriteStrategy>>();
        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
    
    private class TestAnalyticsWriter1: IAnalyticsWriteStrategy
    {
        public Type RequestType { get; }
        public Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
    private class TestAnalyticsWriter2: IAnalyticsWriteStrategy
    {
        public Type RequestType { get; }
        public Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
