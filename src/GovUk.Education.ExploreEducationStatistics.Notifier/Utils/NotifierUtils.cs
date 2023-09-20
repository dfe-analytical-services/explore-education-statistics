#nullable enable
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Utils
{
    public static class NotifierUtils
    {
        public static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
