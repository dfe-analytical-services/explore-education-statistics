using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder SetBasePath(this IConfigurationBuilder builder, IHostEnvironment hostEnvironment)
    {
        Log.Logger.Information("Directory.GetCurrentDirectory() = {CurrentDirectory}", Directory.GetCurrentDirectory());
        Log.Logger.Information("context.HostingEnvironment.ContentRootPath = {ContentRootPath}", hostEnvironment.ContentRootPath);
        if (hostEnvironment.IsProduction())
        {
            Log.Logger.Information("Setting base path to: /home/site/wwwroot");
            builder.SetBasePath("/home/site/wwwroot");
        }

        return builder;
    }
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
                               Configuration file found: "{AppSettingsFullFilename}" - Contents:
                               """, appSettingsFile.FullName);
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
