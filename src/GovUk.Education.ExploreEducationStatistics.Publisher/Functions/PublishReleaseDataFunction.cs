using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusDataStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class PublishReleaseDataFunction
    {
        private readonly IConfiguration _configuration;
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public PublishReleaseDataFunction(IConfiguration configuration,
            IQueueService queueService,
            IReleasePublishingStatusService releasePublishingStatusService)
        {
            _configuration = configuration;
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which publishes the statistics data for a Release by triggering an ADF Pipeline to copy it between databases.
        /// </summary>
        /// <remarks>
        /// Triggers publishing content for the Release if publishing is immediate and the function is running locally where the ADF Pipeline is skipped.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("PublishReleaseData")]
        // ReSharper disable once UnusedMember.Global
        public async Task PublishReleaseData(
            [QueueTrigger(PublishReleaseDataQueue)] PublishReleaseDataMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{0} triggered: {1}",
                executionContext.FunctionName,
                message);

            if (PublisherUtils.IsDevelopment())
            {
                // Skip the ADF Pipeline if running locally
                // If the Release is immediate then trigger publishing the content
                // This usually happens when the ADF Pipeline is complete
                if (await _releasePublishingStatusService.IsImmediate(message.ReleaseId, message.ReleaseStatusId))
                {
                    await _queueService.QueuePublishReleaseContentMessageAsync(message.ReleaseId,
                        message.ReleaseStatusId);
                }

                await UpdateStage(message, Complete);
            }
            else
            {
                try
                {
                    var clientConfiguration = new DataFactoryClientConfiguration(_configuration);
                    var success = TriggerDataFactoryReleasePipeline(clientConfiguration, logger, message);
                    await UpdateStage(message, success ? Started : Failed);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception occured while executing {0}",
                        executionContext.FunctionName);
                    await UpdateStage(message, Failed,
                        new ReleasePublishingStatusLogMessage($"Exception in data stage: {e.Message}"));
                }
            }

            logger.LogInformation("{0} completed",
                executionContext.FunctionName);
        }

        private async Task UpdateStage(PublishReleaseDataMessage message, ReleasePublishingStatusDataStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await _releasePublishingStatusService.UpdateDataStageAsync(message.ReleaseId, message.ReleaseStatusId, stage,
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
                "Pipeline status code: {0}, run Id: {1}",
                runResponse.Response.StatusCode,
                runResponse.Body.RunId);

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
