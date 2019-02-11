using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public class AzureDocumentService : IAzureDocumentService
    {
        private const string databaseName = "education-statistics";

        private readonly DocumentClient _client;

        public AzureDocumentService(IOptions<AzureStorageConfigurationOptions> options)
        {
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
            
            await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                collectionDefinition,
                new RequestOptions());
        }
    }
}