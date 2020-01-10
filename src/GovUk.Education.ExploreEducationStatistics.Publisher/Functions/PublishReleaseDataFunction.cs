using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseDataFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "publish-release-data";
        
        private const string SubscriptionId = "AzureSubscriptionId";
        private const string ResourceGroupName = "AzureResourceGroupName";
        private const string DataFactoryName = "AzureDataFactoryName";
        private const string DataFactoryPipelineName = "AzureDataFactoryPipelineName";
        
        public PublishReleaseDataFunction(IReleaseStatusService releaseStatusService)
        {
            _releaseStatusService = releaseStatusService;
        }

        [FunctionName("PublishReleaseData")]
        public async Task PublishReleaseData(
            [QueueTrigger(QueueName)] PublishReleaseDataMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            var response = await TriggerDataFactoryReleasePipeline(executionContext, message);
            await UpdateStage(message, response.IsSuccessStatusCode ? Started : Failed);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseDataMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateDataStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }
        
        private async Task<HttpResponseMessage> TriggerDataFactoryReleasePipeline(ExecutionContext context, PublishReleaseDataMessage message)
        {
            var config = LoadAppSettings(context);
            var subscriptionId = config.GetValue<string>(SubscriptionId);
            var resourceGroupName = config.GetValue<string>(ResourceGroupName);
            var dataFactoryName = config.GetValue<string>(DataFactoryName);
            var dataFactoryPipelineName = config.GetValue<string>(DataFactoryPipelineName);
            
            Dictionary<string, Guid> postParams = new Dictionary<string, Guid>
            {
                {"releaseId", message.ReleaseId},
                {"releaseStatusId", message.ReleaseStatusId},
                {"themeId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"topicId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"publicationId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"subjectId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
            };

            var json = JsonConvert.SerializeObject(postParams);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DataFactory/factories/{dataFactoryName}/pipelines/{dataFactoryPipelineName}/createRun?api-version=2018-06-01";
            using var client = new HttpClient();

            return await client.PostAsync(url, data);
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