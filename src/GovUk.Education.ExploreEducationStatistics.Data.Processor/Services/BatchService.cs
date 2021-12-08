#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBlobStorageService _blobStorageService;

        public BatchService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<int> GetNumBatchesRemaining(File dataFile)
        {
            var batchFiles = await GetBatchFilesForDataFile(dataFile);
            return batchFiles.Count;
        }

        public async Task<List<BlobInfo>> GetBatchFilesForDataFile(File dataFile)
        {
            return await _blobStorageService.ListBlobs(
                PrivateReleaseFiles,
                dataFile.BatchesPath());
        }
    }
}
