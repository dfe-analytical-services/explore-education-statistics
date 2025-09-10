namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key)
            .ThrowIfBlank(
                configuration is IConfigurationSection configurationSection
                    ? $"{configurationSection.Path}.{key}"
                    : key);
}
