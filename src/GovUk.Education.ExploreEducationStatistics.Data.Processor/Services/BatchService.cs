using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ImportStatus
    {
        RUNNING_PHASE_1 = 1,
        RUNNING_PHASE_2 = 2,
        COMPLETE = 3,
        FAILED = 4
    };
    
    public class BatchService : IBatchService
    {
        private readonly ILogger<IBatchService> _logger;
        private readonly CloudTable _table;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUploadStatusService _uploadStatusService;
        
        public BatchService(
            ITableStorageService tblStorageService,
            IFileStorageService fileStorageService,
            IUploadStatusService uploadStatusService,
            ILogger<IBatchService> logger)
        {
            _table = tblStorageService.GetTableAsync("imports").Result;
            _fileStorageService = fileStorageService;
            _uploadStatusService = uploadStatusService;
            _logger = logger;
        }

        public async Task UpdateBatchCount(string releaseId, string dataFileName, int batchNo)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetCloudBlockBlob(releaseId, dataFileName);
            var leaseId = await _fileStorageService.GetLeaseId(cloudBlockBlob);
            DatafileImport import;
            
            try
            {
                import = GetImport(releaseId, dataFileName).Result;
                var bitArray = new BitArray(import.BatchesProcessed);
                bitArray.Set(batchNo - 1, true);
                bitArray.CopyTo(import.BatchesProcessed, 0);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }
            
            if (await _uploadStatusService.GetPercentageComplete(releaseId, dataFileName) == 100)
            {
                await UpdateStatus(releaseId, dataFileName,
                    import.Errors.Equals("") ? ImportStatus.COMPLETE : ImportStatus.FAILED
                );
        
                _logger.LogInformation(import.Errors.Equals("")
                    ? $"All batches imported for {releaseId} : {dataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {dataFileName} but with errors - check storage log");
            }
        }

        public async Task UpdateStatus(string releaseId, string dataFileName, ImportStatus status)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetCloudBlockBlob(releaseId, dataFileName);
            var leaseId = await _fileStorageService.GetLeaseId(cloudBlockBlob);

            try
            {
                var import = await GetImport(releaseId, dataFileName);
                import.Status = (int)status;
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }
        }

        public async Task FailImport(string releaseId, string dataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, dataFileName);
            import.Status = (int)ImportStatus.FAILED;
            import.Errors = JsonConvert.SerializeObject(errors);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }
        public async Task LogErrors(string releaseId, string dataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, dataFileName);
            if (!import.Errors.Equals(""))
            {
                var currentErrors = JsonConvert.DeserializeObject<List<string>>(import.Errors);
                currentErrors.Concat(errors);
                import.Errors = JsonConvert.SerializeObject(currentErrors);
            }
            else
            {
                import.Errors = JsonConvert.SerializeObject(errors);
            }
            
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }

        public async Task<bool> IsBatchProcessed(string releaseId, string dataFileName, int batchNo)
        {
            var import = await GetImport(releaseId, dataFileName);
            var bitArray = new BitArray(import.BatchesProcessed);
            return bitArray.Get(batchNo - 1);
        }

        public async Task CreateImport(string releaseId, string dataFileName, int numBatches)
        {
            await _uploadStatusService.CreateImport(releaseId, dataFileName, numBatches);
        }

        public async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            return await _uploadStatusService.GetImport(releaseId, dataFileName);
        }
    }
}