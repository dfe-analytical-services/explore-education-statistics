using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key)
            .ThrowIfBlank(
                configuration is IConfigurationSection configurationSection
                    ? $"{configurationSection.Path}.{key}"
                    : key);
}
