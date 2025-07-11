using System.IO;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Config;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics.Extensions;

public class ServiceCollectionExtensionsTests
{
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
            .AddAnalytics(configuration)
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
            .AddAnalytics(configuration)
            .BuildServiceProvider();
        
        
        // Assert
        // If Analytics is enabled, then AddAnalyticsCommon will have registered the real AnalyticsManager. Otherwise it registers the NoOp version.
        var actualAnalyticsManager = serviceProvider.GetRequiredService<IAnalyticsManager>();
        var expectedAnalyticsManager = isAnalyticsEnabled ? typeof(AnalyticsManager) : typeof(NoOpAnalyticsManager);
        Assert.IsType(expectedAnalyticsManager, actualAnalyticsManager);
    }
}
