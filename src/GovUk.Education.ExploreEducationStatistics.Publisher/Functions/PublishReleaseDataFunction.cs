using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
                    var subjects = GetSubjects(message.ReleaseId).ToList();
                    if (subjects.Any())
                    {
                        var response = await TriggerDataFactoryReleasePipeline(executionContext, logger,
                            subjects.First(), message.ReleaseStatusId);
                        await UpdateStage(message, response.IsSuccessStatusCode ? Started : Failed);
                    }
                    else
                    {
                        await UpdateStage(message, Complete);
                    }
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

        private IEnumerable<Subject> GetSubjects(Guid releaseId)
        {
            return _context.Subject
                .Where(s => s.ReleaseId.Equals(releaseId))
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic);
        }

        private static async Task<HttpResponseMessage> TriggerDataFactoryReleasePipeline(ExecutionContext context,
            ILogger logger, Subject subject, Guid releaseStatusId)
        {
            var config = LoadAppSettings(context);
            var subscriptionId = config.GetValue<string>(SubscriptionId);
            var resourceGroupName = config.GetValue<string>(ResourceGroupName);
            var dataFactoryName = config.GetValue<string>(DataFactoryName);
            var dataFactoryPipelineName = config.GetValue<string>(DataFactoryPipelineName);

            var jsonBody = BuildPostParamsJson(subject, releaseStatusId);
            logger.LogInformation($"Triggering data factory: {jsonBody}");

            var data = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);
            var url =
                $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DataFactory/factories/{dataFactoryName}/pipelines/{dataFactoryPipelineName}/createRun?api-version=2018-06-01";
            using var client = new HttpClient();

            return await client.PostAsync(url, data);
        }

        private static string BuildPostParamsJson(Subject subject, Guid releaseStatusId)
        {
            var publication = subject.Release.Publication;
            var topic = publication.Topic;
            var postParams = new Dictionary<string, Guid>
            {
                {"subjectId", subject.Id},
                {"releaseId", subject.ReleaseId},
                {"releaseStatusId", releaseStatusId},
                {"publicationId", publication.Id},
                {"topicId", topic.Id},
                {"themeId", topic.ThemeId}
            };

            return JsonConvert.SerializeObject(postParams);
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