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

        var appSettingsFile = new FileInfo(path);
        if (appSettingsFile.Exists)
        {
            Log.Logger.Information("""
                               Configuration file found: "{AppSettingsFilename}" - Contents:
                               """, path);
            Log.Logger.Information("{FileContents}", appSettingsFile.OpenText().ReadToEnd());
        }
        else
        {
            Log.Logger.Information("""
                               Configuration file not found: "{AppSettingsFilename}"
                               """, path);
        }

        return builder.AddJsonFile(path, optional, reloadOnChange);
    }   
}
