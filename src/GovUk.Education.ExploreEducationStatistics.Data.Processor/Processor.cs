using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Processor
    {
        private const string StorageConnectionName = "AzureStorage";
        private const string ContainerName = "releases";
        private const string UploadsDir = "admin-file-uploads";

        private readonly IProcessorService _processorService;

        public Processor(IProcessorService processorService)
        {
            _processorService = processorService;
        }

        [FunctionName("FilesProcessor")]
        public void FilesProcessorFunc(
            [QueueTrigger("imports-pending", Connection = "")] JObject fNotifyIn,
            [Queue("imports-processed", Connection = "")] out JObject fNotifyOut,
            ILogger logger,
            ExecutionContext context)
        {
            logger.LogInformation($"FilesProcessor function triggered: {fNotifyIn.ToString()}");

            var filesProcessorNotification = ExtractNotification(fNotifyIn);
            var config = LoadAppSettings(context);
            var blobStorageConnectionStr = config.GetConnectionString(StorageConnectionName);

            _processorService.ProcessFiles(filesProcessorNotification, ContainerName, blobStorageConnectionStr, UploadsDir);

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
