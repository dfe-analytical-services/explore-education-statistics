#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Notify.Client;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Utils
{
    public static class NotifierUtils
    {
        // @MarkFix
        //public static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        //{
        //    return new ConfigurationBuilder()
        //        .SetBasePath(context.FunctionAppDirectory)
        //        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        //        .AddEnvironmentVariables()
        //        .Build();
        //}

        // @MarkFix
        //public static NotificationClient GetNotifyClient(IConfiguration config)
        //{
        //    var notifyApiKey = config.GetValue<string>(ConfigKeys.NotifyApiKeyName);
        //    return new NotificationClient(notifyApiKey);
        //}

        // @MarkFix move this?
        public static Task<CloudTable> GetCloudTable(IStorageTableService storageTableService, IConfiguration config,
            string tableName)
        {
            var connectionStr = config.GetValue<string>(StorageConnectionName);
            return storageTableService.GetTable(connectionStr, tableName);
        }
    }
}
