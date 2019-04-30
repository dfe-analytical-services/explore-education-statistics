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
            [QueueTrigger("imports-pending", Connection = "")] JObject fNotifyIn,
            [Queue("imports-processed", Connection = "")] out JObject fNotifyOut,
            [Inject]IProcessorService processorService,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation($"C# Queue trigger function processed: {fNotifyIn.ToString()}");

            var filesProcessorNotification = ExtractNotification(fNotifyIn);
            var config = LoadAppSettings(context);
            var blobStorageConnectionStr = config.GetConnectionString(StorageConnectionName);

            processorService.ProcessFiles(filesProcessorNotification, ContainerName, blobStorageConnectionStr, UploadsDir);

            logger.LogInformation("Completed files processing");

            fNotifyOut = fNotifyIn;
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
