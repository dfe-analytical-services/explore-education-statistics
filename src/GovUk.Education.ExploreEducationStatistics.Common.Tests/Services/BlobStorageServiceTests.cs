using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class BlobStorageServiceTests
    {
        private static BlobStorageService SetupBlobStorageService(
            BlobServiceClient blobServiceClient = null)
        {
            return new BlobStorageService(
                connectionString: "",
                blobServiceClient ?? new Mock<BlobServiceClient>().Object,
                null
            );
        }
    }
}
