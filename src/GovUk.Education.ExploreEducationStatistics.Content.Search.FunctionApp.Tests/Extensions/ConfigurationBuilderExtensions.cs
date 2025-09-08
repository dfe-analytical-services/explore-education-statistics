using System.Text;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonString(this IConfigurationBuilder builder, string json) =>
        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
}
