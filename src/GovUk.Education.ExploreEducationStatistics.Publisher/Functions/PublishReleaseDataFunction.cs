using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class PublishReleaseDataFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly StatisticsDbContext _context;

        public const string QueueName = "publish-release-data";
        private const string ApplicationClientId = "ApplicationClientId";
        private const string AuthenticationKey = "AuthenticationKey";
        private const string DirectoryTenantId = "DirectoryTenantId";
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
                        var success = TriggerDataFactoryReleasePipeline(executionContext, logger, subjects.First(),
                            message.ReleaseStatusId);
                        await UpdateStage(message, success ? Started : Failed);
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

        private static bool TriggerDataFactoryReleasePipeline(ExecutionContext context,
            ILogger logger, Subject subject, Guid releaseStatusId)
        {
            var config = LoadAppSettings(context);
            var applicationId = config.GetValue<string>(ApplicationClientId);
            var authenticationKey = config.GetValue<string>(AuthenticationKey);
            var dataFactoryName = config.GetValue<string>(DataFactoryName);
            var directoryTenantId = config.GetValue<string>(DirectoryTenantId);
            var pipelineName = config.GetValue<string>(DataFactoryPipelineName);
            var resourceGroupName = config.GetValue<string>(ResourceGroupName);
            var subscriptionId = config.GetValue<string>(SubscriptionId);
            var parameters = BuildPipelineParameters(subject, releaseStatusId);
            var client = GetDataFactoryClient(directoryTenantId, applicationId, authenticationKey, subscriptionId);

            var runResponse = client.Pipelines
                .CreateRunWithHttpMessagesAsync(resourceGroupName, dataFactoryName, pipelineName,
                    parameters: parameters).Result;

            logger.LogInformation(
                $"Pipeline status code: {runResponse.Response.StatusCode}, run Id: {runResponse.Body.RunId}");

            return runResponse.Response.IsSuccessStatusCode;
        }

        private static IDictionary<string, object> BuildPipelineParameters(Subject subject, Guid releaseStatusId)
        {
            var publication = subject.Release.Publication;
            var topic = publication.Topic;
            return new Dictionary<string, object>
            {
                {"subjectId", subject.Id},
                {"releaseId", subject.ReleaseId},
                {"releaseStatusId", releaseStatusId},
                {"publicationId", publication.Id},
                {"topicId", topic.Id},
                {"themeId", topic.ThemeId}
            };
        }

        private static DataFactoryManagementClient GetDataFactoryClient(string tenantId, string applicationId,
            string authenticationKey, string subscriptionId)
        {
            var context = new AuthenticationContext("https://login.windows.net/" + tenantId);
            var clientCredential = new ClientCredential(applicationId, authenticationKey);
            var result = context.AcquireTokenAsync("https://management.azure.com/", clientCredential).Result;
            ServiceClientCredentials credentials = new TokenCredentials(result.AccessToken);

            return new DataFactoryManagementClient(credentials)
            {
                SubscriptionId = subscriptionId
            };
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