namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public class DummyAzureDocumentService : IAzureDocumentService
    {
        public void CreatePartitionedCollectionIfNotExists(string id, string partitionKey)
        {
            // Intentionally do nothing
        }
    }
}