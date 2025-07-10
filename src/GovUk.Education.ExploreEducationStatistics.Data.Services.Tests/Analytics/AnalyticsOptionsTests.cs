using System.IO;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Config;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics;

public class AnalyticsOptionsTests
{
    [Theory]
    [InlineData("""
                   {
                       "Analytics":
                       {
                           "Enabled": "true"
                       }
                   }
                   """, true)]
    [InlineData("""
                   {
                       "Analytics":
                       {
                           "Enabled": "false"
                       }
                   }
                   """, false)]
    [InlineData("""
                   {
                   }
                   """, false)]
    public void GivenConfig_WhenTestAnalyticsOptionsIsEnabled_ThenReturnsExpected(string json, bool expected)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
            .Build();

        // Act
        var actual = AnalyticsOptions.IsEnabled(configuration);
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
