using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public class AzureDocumentService : IAzureDocumentService
    {
        private const string databaseName = "education-statistics";
        private readonly ILogger _logger;

        private readonly DocumentClient _client;

        public AzureDocumentService(ILogger<AzureDocumentService> logger, IOptions<AzureStorageConfigurationOptions> options)
        {
            _logger = logger;
            var cosmosEndpointUrl = options.Value.CosmosEndpointUrl;
            var cosmosAuthKey = options.Value.CosmosAuthorizationKey;

            _client = new DocumentClient(new Uri(cosmosEndpointUrl), cosmosAuthKey);
        }

        public async Task CreatePartitionedCollectionIfNotExists(string id, string partitionKey)
        {
            var collectionDefinition = new DocumentCollection
            {
                Id = id
            };

            collectionDefinition.PartitionKey.Paths.Add(partitionKey);
         
            _logger.LogInformation("Creating partitioned collection if not exists {Id}", id);
            
            await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                collectionDefinition,
                new RequestOptions());
        }
    }
}