using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class BlobLease
    {
        private readonly BlobLeaseClient _leaseClient;

        public BlobLease(BlobLeaseClient leaseClient)
        {
            _leaseClient = leaseClient;
        }

        public async Task Release()
        {
            await _leaseClient.ReleaseAsync();
        }
    }
}