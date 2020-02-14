using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.WebJobs;
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

            if (IsDevelopment())
            {
                // Skip Data Factory
                await UpdateStage(message, Complete);
            }
            else
            {
                try
                {
                    var configuration = LoadClientConfiguration(executionContext);
                    var success = TriggerDataFactoryReleasePipeline(configuration, logger, message);
                    await UpdateStage(message, success ? Started : Failed);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception occured while executing {executionContext.FunctionName}");
                    await UpdateStage(message, Failed,
                        new ReleaseStatusLogMessage($"Exception in data stage: {e.Message}"));
                }
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task UpdateStage(PublishReleaseDataMessage message, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await _releaseStatusService.UpdateDataStageAsync(message.ReleaseId, message.ReleaseStatusId, stage,
                logMessage);
        }

        private static bool TriggerDataFactoryReleasePipeline(DataFactoryClientConfiguration configuration,
            ILogger logger, PublishReleaseDataMessage message)
        {
            var parameters = BuildPipelineParameters(message);
            var client = GetDataFactoryClient(configuration);

            var runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(configuration.ResourceGroupName,
                configuration.DataFactoryName, configuration.PipelineName, parameters: parameters).Result;

            logger.LogInformation(
                $"Pipeline status code: {runResponse.Response.StatusCode}, run Id: {runResponse.Body.RunId}");

            return runResponse.Response.IsSuccessStatusCode;
        }

        private static IDictionary<string, object> BuildPipelineParameters(PublishReleaseDataMessage message)
        {
            return new Dictionary<string, object>
            {
                {"releaseId", message.ReleaseId},
                {"releaseStatusId", message.ReleaseStatusId}
            };
        }

        private static DataFactoryManagementClient GetDataFactoryClient(DataFactoryClientConfiguration configuration)
        {
            var context = new AuthenticationContext($"https://login.windows.net/{configuration.TenantId}");
            var clientCredential = new ClientCredential(configuration.ClientId, configuration.ClientSecret);
            var result = context.AcquireTokenAsync("https://management.azure.com/", clientCredential).Result;
            ServiceClientCredentials credentials = new TokenCredentials(result.AccessToken);

            return new DataFactoryManagementClient(credentials)
            {
                SubscriptionId = configuration.SubscriptionId
            };
        }

        private static DataFactoryClientConfiguration LoadClientConfiguration(ExecutionContext context)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            return new DataFactoryClientConfiguration(configuration);
        }

        private static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            return environment?.Equals(EnvironmentName.Development) ?? false;
        }

        private class DataFactoryClientConfiguration
        {
            public string ClientId { get; }
            public string ClientSecret { get; }
            public string DataFactoryName { get; }
            public string PipelineName { get; }
            public string ResourceGroupName { get; }
            public string SubscriptionId { get; }
            public string TenantId { get; }

            public DataFactoryClientConfiguration(IConfiguration configuration)
            {
                ClientId = configuration.GetValue<string>(nameof(ClientId));
                ClientSecret = configuration.GetValue<string>(nameof(ClientSecret));
                DataFactoryName = configuration.GetValue<string>(nameof(DataFactoryName));
                PipelineName = configuration.GetValue<string>(nameof(PipelineName));
                ResourceGroupName = configuration.GetValue<string>(nameof(ResourceGroupName));
                SubscriptionId = configuration.GetValue<string>(nameof(SubscriptionId));
                TenantId = configuration.GetValue<string>(nameof(TenantId));
            }
        }
    }
}