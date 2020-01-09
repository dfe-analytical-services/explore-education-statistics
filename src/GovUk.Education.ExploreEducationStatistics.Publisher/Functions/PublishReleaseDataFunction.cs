using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseDataFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;

        public const string QueueName = "publish-release-data";

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
            await TriggerDataFactoryReleasePipeline(message);
            await UpdateStage(message, Started);
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseDataMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateDataStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }
        
        private async Task TriggerDataFactoryReleasePipeline(PublishReleaseDataMessage message)
        {
            // TODO - get the additional params to pass to pipeline
            Dictionary<string, Guid> postParams = new Dictionary<string, Guid>
            {
                {"releaseId", message.ReleaseId},
                {"releaseStatusId", message.ReleaseStatusId},
                {"themeId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"topicId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"publicationId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
                {"subjectId", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")},
            };
            
            var subscriptionId = "Guid - derive from running function possibly";
            var resourceGroupName = "Azure resource group name";
            var factoryName = "s101d01datafactory";
            var pipelineName = "pl_release_statistics";
            
            var json = JsonConvert.SerializeObject(postParams);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DataFactory/factories/{factoryName}/pipelines/{pipelineName}/createRun?api-version=2018-06-01";
            using var client = new HttpClient();

            var response = await client.PostAsync(url, data);

            // TODO Check result etc
            string result = response.Content.ReadAsStringAsync().Result;
        }
    }
}