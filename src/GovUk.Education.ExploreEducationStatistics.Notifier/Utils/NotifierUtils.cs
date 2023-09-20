#nullable enable
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;

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

        public static CloudTable GetCloudTable(IStorageTableService storageTableService, IConfiguration config,
            string tableName)
        {
            var connectionStr = config.GetValue<string>(StorageConnectionName);
            return storageTableService.GetTable(connectionStr, tableName).Result;
        }
    }
}
