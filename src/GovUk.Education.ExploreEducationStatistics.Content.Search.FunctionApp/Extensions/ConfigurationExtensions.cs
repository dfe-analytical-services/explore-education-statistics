using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// When running in Azure, the default path from which it attempts to load appsettings.Production.json is wrong.
    /// context.HostingEnvironment.ContentRootPath = "/azure-functions-host"
    /// However, the file resides in the current directory, "/home/site/wwwroot".
    /// See: https://stackoverflow.com/questions/78119200/appsettings-for-azurefunction-on-net-8-isolated
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    public static IConfigurationBuilder SetBasePath(this IConfigurationBuilder builder, IHostEnvironment hostEnvironment) => 
        hostEnvironment.IsProduction() 
            ? builder.SetBasePath(Directory.GetCurrentDirectory()) // "/home/site/wwwroot" 
            : builder;
}
