using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public class DummyAzureDocumentService : IAzureDocumentService
    {
        public async Task CreatePartitionedCollectionIfNotExists(string id, string partitionKey)
        {
            // Intentionally do nothing
        }
    }
}