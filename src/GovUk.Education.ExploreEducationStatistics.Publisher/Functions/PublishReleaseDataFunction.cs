using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseDataFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly StatisticsDbContext _context;

        public const string QueueName = "publish-release-data";

        private const string SubscriptionId = "AzureSubscriptionId";
        private const string ResourceGroupName = "AzureResourceGroupName";
        private const string DataFactoryName = "AzureDataFactoryName";
        private const string DataFactoryPipelineName = "AzureDataFactoryPipelineName";

        public PublishReleaseDataFunction(
            StatisticsDbContext context,
            IReleaseStatusService releaseStatusService)
        {
            _releaseStatusService = releaseStatusService;
            _context = context;
        }

        [FunctionName("PublishReleaseData")]
        public async Task PublishReleaseData(
            [QueueTrigger(QueueName)] PublishReleaseDataMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            
            if (IsDevelopment())
            {
                // Skip Data Factory
                await UpdateStage(message, Complete);
            }
            else
            {
                try
                {
                    var response = await TriggerDataFactoryReleasePipeline(executionContext, message);
                    await UpdateStage(message, response.IsSuccessStatusCode ? Started : Failed);   
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                    await UpdateStage(message, Failed);
                }
            }
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseDataMessage message, Stage stage)
        {
            await _releaseStatusService.UpdateDataStageAsync(message.ReleaseId, message.ReleaseStatusId, stage);
        }

        private async Task<HttpResponseMessage> TriggerDataFactoryReleasePipeline(ExecutionContext context,
            PublishReleaseDataMessage message)
        {
            var config = LoadAppSettings(context);
            var subscriptionId = config.GetValue<string>(SubscriptionId);
            var resourceGroupName = config.GetValue<string>(ResourceGroupName);
            var dataFactoryName = config.GetValue<string>(DataFactoryName);
            var dataFactoryPipelineName = config.GetValue<string>(DataFactoryPipelineName);

            var subject = _context.Subject
                .Where(s => s.Id.Equals(message.ReleaseId))
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .FirstOrDefault();

            var postParams = new Dictionary<string, Guid>
            {
                {"subjectId", subject.Id},
                {"releaseId", subject.ReleaseId},
                {"releaseStatusId", message.ReleaseStatusId},
                {"publicationId", subject.Release.PublicationId},
                {"topicId", subject.Release.Publication.TopicId},
                {"themeId", subject.Release.Publication.Topic.ThemeId},
            };

            var json = JsonConvert.SerializeObject(postParams);
            var data = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var url =
                $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DataFactory/factories/{dataFactoryName}/pipelines/{dataFactoryPipelineName}/createRun?api-version=2018-06-01";
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

        private static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            return environment?.Equals(EnvironmentName.Development) ?? false;
        }
    }
}