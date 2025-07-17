using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    
        public static TheoryData<string, bool> Configs = new()
    {
        {
            """
            {
                "Analytics":
                {
                    "Enabled": "true"
                }
            }
            """, true
        },
        {
            """
            {
                "Analytics":
                {
                    "Enabled": "false"
                }
            }
            """, false
        },
        {
            """
            {
            }
            """, false
        }
    };

    [Theory]
    [MemberData(nameof(Configs))]
    public void GivenConfig_WhenAddAnalyticsCalled_ThenRegistersAndBindsAnalyticsOptions(string json, bool isAnalyticsEnabled)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddOptions()
            .AddAnalyticsCommon(configuration)
            .WhenEnabled
            .Services
            .BuildServiceProvider();
        
        // Assert
        var actual = serviceProvider.GetRequiredService<IOptions<AnalyticsOptions>>();
        Assert.Equal(isAnalyticsEnabled, actual.Value.Enabled);
    }
    
    [Theory]
    [MemberData(nameof(Configs))]
    public void GivenConfig_WhenAddAnalyticsCalled_ThenAddsAnalyticsCommonSpecifyingWhetherAnalyticsIsEnabled(string json, bool isAnalyticsEnabled)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddOptions()
            .AddAnalyticsCommon(configuration)
            .WhenEnabled
            .Services
            .BuildServiceProvider();
        
        
        // Assert
        // If Analytics is enabled, then AddAnalyticsCommon will have registered the real AnalyticsManager. Otherwise it registers the NoOp version.
        var actualAnalyticsManager = serviceProvider.GetRequiredService<IAnalyticsManager>();
        var expectedAnalyticsManager = isAnalyticsEnabled ? typeof(AnalyticsManager) : typeof(NoOpAnalyticsManager);
        Assert.IsType(expectedAnalyticsManager, actualAnalyticsManager);
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
