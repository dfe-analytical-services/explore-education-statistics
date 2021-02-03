using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobStorageService _blobStorageService;

        public BatchService(ContentDbContext contentDbContext,
            IBlobStorageService blobStorageService)
        {
            _contentDbContext = contentDbContext;
            _blobStorageService = blobStorageService;
        }

        public async Task<int> GetNumBatchesRemaining(Guid fileId)
        {
            var batchFiles = await GetBatchFilesForDataFile(fileId);
            return batchFiles.Count;
        }

        public async Task<List<BlobInfo>> GetBatchFilesForDataFile(Guid fileId)
        {
            var file = await _contentDbContext.Files.FindAsync(fileId);

            var blobs = await _blobStorageService.ListBlobs(
                PrivateFilesContainerName,
                AdminReleaseBatchesDirectoryPath(file.ReleaseId)
            );

            return blobs.Where(blob =>
                    IsBatchFileForDataFile(file.ReleaseId, file.Filename, blob.Path))
                .ToList();
        }
    }
}