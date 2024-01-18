using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public IConfigurationRoot Get(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
