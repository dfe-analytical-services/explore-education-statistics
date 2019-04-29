namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    using GovUK.Education.ExploreEducationStatistics.Data.Processor.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

    public class Processor
    {
        private const string StorageConnectionName = "BlobStorageConnString";
        private const string ContainerName = "releases";
        private const string UploadsDir = "admin-file-uploads";

        [FunctionName("FilesProcessor")]
        public static void FilesProcessorFunc(
            [QueueTrigger("imports-pending", Connection = "")] JObject fNotify,
            [Queue("imports-processed", Connection = "")] out JObject fNotifyOut,
            [Inject]IProcessorService processorService,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed: {fNotify.ToString()}");

            var filesProcessorNotification = ExtractNotification(fNotify);
            var config = LoadAppSettings(context);
            var blobStorageConnectionStr = config.GetConnectionString(StorageConnectionName);

            processorService.ProcessFiles(filesProcessorNotification, ContainerName, blobStorageConnectionStr, UploadsDir);

            log.LogInformation("Completed files processing");

            fNotifyOut = fNotify;
        }

        private static ProcessorNotification ExtractNotification(JObject processorNotification)
        {
            return processorNotification.ToObject<ProcessorNotification>();
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
