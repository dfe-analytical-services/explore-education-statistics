namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public interface IAzureDocumentService
    {
        void CreatePartitionedCollectionIfNotExists(string id, string partitionKey);
    }
}