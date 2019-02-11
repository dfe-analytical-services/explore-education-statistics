using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Azure
{
    public interface IAzureDocumentService
    {
        Task CreatePartitionedCollectionIfNotExists(string id, string partitionKey);
    }
}