using Microsoft.Extensions.Configuration;
using Serilog;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFileAndLog(
        this IConfigurationBuilder builder,
        string path,
        bool optional,
        bool reloadOnChange)
    {
        Log.Logger.Information("""
                               Loading configuration from "{AppSettingsFilename}"
                               """, path);
        return builder.AddJsonFile(path, optional, reloadOnChange);
    }   
}
